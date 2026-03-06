using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Utm_market.Core.Entities;
using Utm_market.Core.Filters;
using Utm_market.Core.Repositories;
using Utm_market.Infrastructure.Data;
using Utm_market.Infrastructure.Mappers;
using Utm_market.Infrastructure.Models.Data;
using ArgumentNullException = System.ArgumentNullException;

namespace Utm_market.Infrastructure.Repositories;

/// <summary>
/// Implementación concreta de <see cref="ISaleRepository"/> para SQL Server,
/// optimizada para Native AOT utilizando mapeo manual con <see cref="SqlDataReader"/>
/// y evitando problemas de N+1 consultas para los detalles de venta.
/// </summary>
public class SaleRepositoryImpl(IDbConnectionFactory connectionFactory) : ISaleRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    /// <summary>
    /// Mapea un <see cref="SqlDataReader"/> a una secuencia de <see cref="VentaEntity"/>.
    /// </summary>
    private static async IAsyncEnumerable<VentaEntity> MapToVentaEntityAsync(
        DbDataReader reader,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (await reader.ReadAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var ventaId = reader.GetInt32(reader.GetOrdinal("VentaID"));
            var folio = reader.GetString(reader.GetOrdinal("Folio"));
            var fechaVenta = reader.GetDateTime(reader.GetOrdinal("FechaVenta"));
            var totalArticulos = reader.GetInt32(reader.GetOrdinal("TotalArticulos"));
            var totalVenta = reader.GetDecimal(reader.GetOrdinal("TotalVenta"));
            var estatus = reader.GetByte(reader.GetOrdinal("Estatus"));

            yield return new VentaEntity(ventaId, folio)
            {
                FechaVenta = fechaVenta,
                TotalArticulos = totalArticulos,
                TotalVenta = totalVenta,
                Estatus = estatus
            };
        }
    }

    /// <summary>
    /// Mapea un <see cref="SqlDataReader"/> a una secuencia de <see cref="DetalleVentaEntity"/>.
    /// </summary>
    private static async IAsyncEnumerable<DetalleVentaEntity> MapToDetalleVentaEntityAsync(
        DbDataReader reader,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (await reader.ReadAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var detalleId = reader.GetInt32(reader.GetOrdinal("DetalleID"));
            var ventaId = reader.GetInt32(reader.GetOrdinal("VentaID"));
            var productoId = reader.GetInt32(reader.GetOrdinal("ProductoID"));
            var precioUnitario = reader.GetDecimal(reader.GetOrdinal("PrecioUnitario"));
            var cantidad = reader.GetInt32(reader.GetOrdinal("Cantidad"));
            var totalDetalle = reader.GetDecimal(reader.GetOrdinal("TotalDetalle"));

            yield return new DetalleVentaEntity(detalleId, ventaId, productoId)
            {
                PrecioUnitario = precioUnitario,
                Cantidad = cantidad,
                TotalDetalle = totalDetalle
            };
        }
    }

    public async IAsyncEnumerable<Sale> GetAllAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var commandVentas = connection.CreateCommand();
        commandVentas.CommandText = "SELECT VentaID, Folio, FechaVenta, TotalArticulos, TotalVenta, Estatus FROM Venta";

        var ventas = new List<VentaEntity>();
        await using (var readerVentas = await ((DbCommand)commandVentas).ExecuteReaderAsync(cancellationToken))
        {
            await foreach (var venta in MapToVentaEntityAsync(readerVentas, cancellationToken))
            {
                ventas.Add(venta);
            }
        }

        if (ventas.Count == 0) yield break;

        var ventaIds = string.Join(",", ventas.Select(v => v.VentaID));
        using var commandDetalles = connection.CreateCommand();
        commandDetalles.CommandText = $"SELECT DetalleID, VentaID, ProductoID, PrecioUnitario, Cantidad, TotalDetalle FROM DetalleVenta WHERE VentaID IN ({ventaIds})";

        var detallesPorVenta = new Dictionary<int, List<DetalleVentaEntity>>();
        await using (var readerDetalles = await ((DbCommand)commandDetalles).ExecuteReaderAsync(cancellationToken))
        {
            await foreach (var detalle in MapToDetalleVentaEntityAsync(readerDetalles, cancellationToken))
            {
                if (!detallesPorVenta.TryGetValue(detalle.VentaID, out var list))
                {
                    list = new List<DetalleVentaEntity>();
                    detallesPorVenta[detalle.VentaID] = list;
                }
                list.Add(detalle);
            }
        }

        foreach (var venta in ventas)
        {
            yield return venta.ToDomain(detallesPorVenta.GetValueOrDefault(venta.VentaID, new List<DetalleVentaEntity>()));
        }
    }

    public async Task<Sale?> GetByIdAsync(int saleId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        // Obtener la cabecera de la venta
        using var commandVenta = connection.CreateCommand();
        commandVenta.CommandText = "SELECT VentaID, Folio, FechaVenta, TotalArticulos, TotalVenta, Estatus FROM Venta WHERE VentaID = @SaleId";
        commandVenta.Parameters.Add(new SqlParameter("@SaleId", SqlDbType.Int) { Value = saleId });

        VentaEntity? venta = null;
        await using (var readerVenta = await ((DbCommand)commandVenta).ExecuteReaderAsync(cancellationToken))
        {
            await foreach (var ve in MapToVentaEntityAsync(readerVenta, cancellationToken))
            {
                venta = ve;
                break; // Solo esperamos una venta
            }
        }

        if (venta == null) return null;

        // Obtener los detalles de la venta
        using var commandDetalles = connection.CreateCommand();
        commandDetalles.CommandText = "SELECT DetalleID, VentaID, ProductoID, PrecioUnitario, Cantidad, TotalDetalle FROM DetalleVenta WHERE VentaID = @SaleId";
        commandDetalles.Parameters.Add(new SqlParameter("@SaleId", SqlDbType.Int) { Value = saleId });

        var detalles = new List<DetalleVentaEntity>();
        await using (var readerDetalles = await ((DbCommand)commandDetalles).ExecuteReaderAsync(cancellationToken))
        {
            await foreach (var detalle in MapToDetalleVentaEntityAsync(readerDetalles, cancellationToken))
            {
                detalles.Add(detalle);
            }
        }

        return venta.ToDomain(detalles);
    }

    public async IAsyncEnumerable<Sale> FindAsync(
        SaleFilter filter, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var commandVentas = connection.CreateCommand();

        var whereClauses = new List<string>();
        if (!string.IsNullOrWhiteSpace(filter.Folio))
        {
            whereClauses.Add("Folio = @Folio");
            commandVentas.Parameters.Add(new SqlParameter("@Folio", SqlDbType.VarChar) { Value = filter.Folio });
        }
        if (filter.Status.HasValue)
        {
            whereClauses.Add("Estatus = @Estatus");
            commandVentas.Parameters.Add(new SqlParameter("@Estatus", SqlDbType.TinyInt) { Value = filter.Status.Value.ToEntityEstatusByte() });
        }
        if (filter.StartDate.HasValue)
        {
            whereClauses.Add("FechaVenta >= @StartDate");
            commandVentas.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = filter.StartDate.Value });
        }
        if (filter.EndDate.HasValue)
        {
            whereClauses.Add("FechaVenta <= @EndDate");
            commandVentas.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = filter.EndDate.Value });
        }
        if (filter.ProductIdInDetails.HasValue)
        {
            // Subconsulta para filtrar ventas que contengan el producto en sus detalles
            whereClauses.Add("VentaID IN (SELECT VentaID FROM DetalleVenta WHERE ProductoID = @ProductIdInDetails)");
            commandVentas.Parameters.Add(new SqlParameter("@ProductIdInDetails", SqlDbType.Int) { Value = filter.ProductIdInDetails.Value });
        }

        commandVentas.CommandText = "SELECT VentaID, Folio, FechaVenta, TotalArticulos, TotalVenta, Estatus FROM Venta";
        if (whereClauses.Count > 0)
        {
            commandVentas.CommandText += " WHERE " + string.Join(" AND ", whereClauses);
        }

        var ventas = new List<VentaEntity>();
        await using (var readerVentas = await ((DbCommand)commandVentas).ExecuteReaderAsync(cancellationToken))
        {
            await foreach (var venta in MapToVentaEntityAsync(readerVentas, cancellationToken))
            {
                ventas.Add(venta);
            }
        }

        if (ventas.Count == 0) yield break;

        var ventaIds = string.Join(",", ventas.Select(v => v.VentaID));
        using var commandDetalles = connection.CreateCommand();
        commandDetalles.CommandText = $"SELECT DetalleID, VentaID, ProductoID, PrecioUnitario, Cantidad, TotalDetalle FROM DetalleVenta WHERE VentaID IN ({ventaIds})";

        var detallesPorVenta = new Dictionary<int, List<DetalleVentaEntity>>();
        await using (var readerDetalles = await ((DbCommand)commandDetalles).ExecuteReaderAsync(cancellationToken))
        {
            await foreach (var detalle in MapToDetalleVentaEntityAsync(readerDetalles, cancellationToken))
            {
                if (!detallesPorVenta.TryGetValue(detalle.VentaID, out var list))
                {
                    list = new List<DetalleVentaEntity>();
                    detallesPorVenta[detalle.VentaID] = list;
                }
                list.Add(detalle);
            }
        }

        foreach (var venta in ventas)
        {
            yield return venta.ToDomain(detallesPorVenta.GetValueOrDefault(venta.VentaID, new List<DetalleVentaEntity>()));
        }
    }

    public async Task<Sale> AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sale);

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = (connection as SqlConnection)!.BeginTransaction();

        try
        {
            var (ventaEntity, detalleEntities) = sale.ToEntity();

            // Insertar Venta
            using var commandVenta = connection.CreateCommand();
            commandVenta.Transaction = transaction;
            commandVenta.CommandText = @"
                INSERT INTO Venta (Folio, FechaVenta, TotalArticulos, TotalVenta, Estatus)
                VALUES (@Folio, @FechaVenta, @TotalArticulos, @TotalVenta, @Estatus);
                SELECT SCOPE_IDENTITY();";

            commandVenta.Parameters.Add(new SqlParameter("@Folio", SqlDbType.VarChar) { Value = ventaEntity.Folio });
            commandVenta.Parameters.Add(new SqlParameter("@FechaVenta", SqlDbType.DateTime) { Value = ventaEntity.FechaVenta });
            commandVenta.Parameters.Add(new SqlParameter("@TotalArticulos", SqlDbType.Int) { Value = ventaEntity.TotalArticulos });
            commandVenta.Parameters.Add(new SqlParameter("@TotalVenta", SqlDbType.Decimal) { Value = ventaEntity.TotalVenta });
            commandVenta.Parameters.Add(new SqlParameter("@Estatus", SqlDbType.TinyInt) { Value = ventaEntity.Estatus });

            var newVentaId = Convert.ToInt32(await ((DbCommand)commandVenta).ExecuteScalarAsync(cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve new sale ID."));

            // Insertar Detalles de Venta
            foreach (var detalle in detalleEntities)
            {
                using var commandDetalle = connection.CreateCommand();
                commandDetalle.Transaction = transaction;
                commandDetalle.CommandText = @"
                    INSERT INTO DetalleVenta (VentaID, ProductoID, PrecioUnitario, Cantidad, TotalDetalle)
                    VALUES (@VentaID, @ProductoID, @PrecioUnitario, @Cantidad, @TotalDetalle);";

                commandDetalle.Parameters.Add(new SqlParameter("@VentaID", SqlDbType.Int) { Value = newVentaId });
                commandDetalle.Parameters.Add(new SqlParameter("@ProductoID", SqlDbType.Int) { Value = detalle.ProductoID });
                commandDetalle.Parameters.Add(new SqlParameter("@PrecioUnitario", SqlDbType.Decimal) { Value = detalle.PrecioUnitario });
                commandDetalle.Parameters.Add(new SqlParameter("@Cantidad", SqlDbType.Int) { Value = detalle.Cantidad });
                commandDetalle.Parameters.Add(new SqlParameter("@TotalDetalle", SqlDbType.Decimal) { Value = detalle.TotalDetalle });

                await ((DbCommand)commandDetalle).ExecuteNonQueryAsync(cancellationToken);
            }

            transaction.Commit();

            // Reconstruir y retornar el objeto Sale con el ID generado
            // Crear una nueva instancia ya que Sale es inmutable (con init-only setters)
            return new Sale(newVentaId, sale.Folio, sale.Status)
            {
                SaleDate = sale.SaleDate,
                SaleDetails = sale.SaleDetails.Select(d => new SaleDetail(
                    d.SaleDetailID, d.ProductID, d.Quantity, d.UnitPrice)).ToList() // SaleDetailID will be 0 for new items
            };
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sale);

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = (connection as SqlConnection)!.BeginTransaction();

        try
        {
            var (ventaEntity, detalleEntities) = sale.ToEntity();

            // Actualizar Venta
            using var commandUpdateVenta = connection.CreateCommand();
            commandUpdateVenta.Transaction = transaction;
            commandUpdateVenta.CommandText = @"
                UPDATE Venta
                SET Folio = @Folio,
                    FechaVenta = @FechaVenta,
                    TotalArticulos = @TotalArticulos,
                    TotalVenta = @TotalVenta,
                    Estatus = @Estatus
                WHERE VentaID = @VentaID;";

            commandUpdateVenta.Parameters.Add(new SqlParameter("@Folio", SqlDbType.VarChar) { Value = ventaEntity.Folio });
            commandUpdateVenta.Parameters.Add(new SqlParameter("@FechaVenta", SqlDbType.DateTime) { Value = ventaEntity.FechaVenta });
            commandUpdateVenta.Parameters.Add(new SqlParameter("@TotalArticulos", SqlDbType.Int) { Value = ventaEntity.TotalArticulos });
            commandUpdateVenta.Parameters.Add(new SqlParameter("@TotalVenta", SqlDbType.Decimal) { Value = ventaEntity.TotalVenta });
            commandUpdateVenta.Parameters.Add(new SqlParameter("@Estatus", SqlDbType.TinyInt) { Value = ventaEntity.Estatus });
            commandUpdateVenta.Parameters.Add(new SqlParameter("@VentaID", SqlDbType.Int) { Value = ventaEntity.VentaID });

            var rowsAffected = await ((DbCommand)commandUpdateVenta).ExecuteNonQueryAsync(cancellationToken);
            if (rowsAffected == 0)
            {
                transaction.Rollback();
                return false; // Venta no encontrada para actualizar
            }

            // Eliminar detalles existentes para la venta
            using var commandDeleteDetalles = connection.CreateCommand();
            commandDeleteDetalles.Transaction = transaction;
            commandDeleteDetalles.CommandText = "DELETE FROM DetalleVenta WHERE VentaID = @VentaID";
            commandDeleteDetalles.Parameters.Add(new SqlParameter("@VentaID", SqlDbType.Int) { Value = ventaEntity.VentaID });
            await ((DbCommand)commandDeleteDetalles).ExecuteNonQueryAsync(cancellationToken);

            // Insertar nuevos/actualizados detalles
            foreach (var detalle in detalleEntities)
            {
                using var commandInsertDetalle = connection.CreateCommand();
                commandInsertDetalle.Transaction = transaction;
                commandInsertDetalle.CommandText = @"
                    INSERT INTO DetalleVenta (VentaID, ProductoID, PrecioUnitario, Cantidad, TotalDetalle)
                    VALUES (@VentaID, @ProductoID, @PrecioUnitario, @Cantidad, @TotalDetalle);";

                commandInsertDetalle.Parameters.Add(new SqlParameter("@VentaID", SqlDbType.Int) { Value = ventaEntity.VentaID });
                commandInsertDetalle.Parameters.Add(new SqlParameter("@ProductoID", SqlDbType.Int) { Value = detalle.ProductoID });
                commandInsertDetalle.Parameters.Add(new SqlParameter("@PrecioUnitario", SqlDbType.Decimal) { Value = detalle.PrecioUnitario });
                commandInsertDetalle.Parameters.Add(new SqlParameter("@Cantidad", SqlDbType.Int) { Value = detalle.Cantidad });
                commandInsertDetalle.Parameters.Add(new SqlParameter("@TotalDetalle", SqlDbType.Decimal) { Value = detalle.TotalDetalle });

                await ((DbCommand)commandInsertDetalle).ExecuteNonQueryAsync(cancellationToken);
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int saleId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = (connection as SqlConnection)!.BeginTransaction();

        try
        {
            // Eliminar detalles de la venta
            using var commandDeleteDetalles = connection.CreateCommand();
            commandDeleteDetalles.Transaction = transaction;
            commandDeleteDetalles.CommandText = "DELETE FROM DetalleVenta WHERE VentaID = @VentaID";
            commandDeleteDetalles.Parameters.Add(new SqlParameter("@VentaID", SqlDbType.Int) { Value = saleId });
            await ((DbCommand)commandDeleteDetalles).ExecuteNonQueryAsync(cancellationToken);

            // Eliminar la cabecera de la venta
            using var commandDeleteVenta = connection.CreateCommand();
            commandDeleteVenta.Transaction = transaction;
            commandDeleteVenta.CommandText = "DELETE FROM Venta WHERE VentaID = @VentaID";
            commandDeleteVenta.Parameters.Add(new SqlParameter("@VentaID", SqlDbType.Int) { Value = saleId });

            var rowsAffected = await ((DbCommand)commandDeleteVenta).ExecuteNonQueryAsync(cancellationToken);
            
            transaction.Commit();
            return rowsAffected > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(int saleId, SaleStatus newStatus, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        const string sql = "UPDATE Venta SET Estatus = @Estatus WHERE VentaID = @SaleId";
        
        var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(
            sql, 
            new { Estatus = newStatus.ToEntityEstatusByte(), SaleId = saleId }, 
            cancellationToken: cancellationToken));
            
        return rowsAffected > 0;
    }
}
