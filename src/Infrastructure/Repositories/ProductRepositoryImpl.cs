using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;
using Utm_market.Core.Filters;
using Utm_market.Core.Repositories;
using Utm_market.Infrastructure.Data;
using Utm_market.Infrastructure.Mappers;
using Utm_market.Infrastructure.Models.Data;

namespace Utm_market.Infrastructure.Repositories;

/// <summary>
/// Implementación concreta de <see cref="IProductRepository"/> para SQL Server,
/// optimizada para Native AOT utilizando mapeo manual con <see cref="SqlDataReader"/>.
/// </summary>
public class ProductRepositoryImpl(IDbConnectionFactory connectionFactory) : IProductRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    /// <summary>
    /// Mapea un <see cref="SqlDataReader"/> a una secuencia de <see cref="ProductoEntity"/>.
    /// </summary>
    private static async IAsyncEnumerable<ProductoEntity> MapToProductoEntityAsync(
        DbDataReader reader,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (await reader.ReadAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var productoId = reader.GetInt32(reader.GetOrdinal("ProductoID"));
            var nombre = reader.GetString(reader.GetOrdinal("Nombre"));
            var sku = reader.GetString(reader.GetOrdinal("SKU"));
            var marca = reader.IsDBNull(reader.GetOrdinal("Marca")) ? null : reader.GetString(reader.GetOrdinal("Marca"));
            var precio = reader.GetDecimal(reader.GetOrdinal("Precio"));
            var stock = reader.GetInt32(reader.GetOrdinal("Stock"));

            yield return new ProductoEntity(productoId, nombre, sku)
            {
                Marca = marca,
                Precio = precio,
                Stock = stock
            };
        }
    }

    public async IAsyncEnumerable<Product> GetAllAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT ProductoID, Nombre, SKU, Marca, Precio, Stock FROM Producto";
        
        using var reader = await ((DbCommand)command).ExecuteReaderAsync(cancellationToken);
        await foreach (var entity in MapToProductoEntityAsync(reader, cancellationToken))
        {
            yield return entity.ToDomain();
        }
    }

    public async Task<Product?> GetByIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT ProductoID, Nombre, SKU, Marca, Precio, Stock FROM Producto WHERE ProductoID = @ProductId";
        command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });

        using var reader = await ((DbCommand)command).ExecuteReaderAsync(cancellationToken);
        await foreach (var entity in MapToProductoEntityAsync(reader, cancellationToken))
        {
            return entity.ToDomain();
        }
        return null;
    }

    public async IAsyncEnumerable<Product> FindAsync(
        ProductFilter filter, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();
        
        var whereClauses = new List<string>();
        if (!string.IsNullOrWhiteSpace(filter.NameContains))
        {
            whereClauses.Add("Nombre LIKE '%' + @NameContains + '%'");
            command.Parameters.Add(new SqlParameter("@NameContains", SqlDbType.NVarChar) { Value = filter.NameContains });
        }
        if (!string.IsNullOrWhiteSpace(filter.SKU))
        {
            whereClauses.Add("SKU = @SKU");
            command.Parameters.Add(new SqlParameter("@SKU", SqlDbType.VarChar) { Value = filter.SKU });
        }
        if (!string.IsNullOrWhiteSpace(filter.Brand))
        {
            whereClauses.Add("Marca = @Brand");
            command.Parameters.Add(new SqlParameter("@Brand", SqlDbType.NVarChar) { Value = filter.Brand });
        }
        if (filter.MinPrice.HasValue)
        {
            whereClauses.Add("Precio >= @MinPrice");
            command.Parameters.Add(new SqlParameter("@MinPrice", SqlDbType.Decimal) { Value = filter.MinPrice.Value });
        }
        if (filter.MaxPrice.HasValue)
        {
            whereClauses.Add("Precio <= @MaxPrice");
            command.Parameters.Add(new SqlParameter("@MaxPrice", SqlDbType.Decimal) { Value = filter.MaxPrice.Value });
        }
        if (filter.MinStock.HasValue)
        {
            whereClauses.Add("Stock >= @MinStock");
            command.Parameters.Add(new SqlParameter("@MinStock", SqlDbType.Int) { Value = filter.MinStock.Value });
        }
        if (filter.MaxStock.HasValue)
        {
            whereClauses.Add("Stock <= @MaxStock");
            command.Parameters.Add(new SqlParameter("@MaxStock", SqlDbType.Int) { Value = filter.MaxStock.Value });
        }

        command.CommandText = "SELECT ProductoID, Nombre, SKU, Marca, Precio, Stock FROM Producto";
        if (whereClauses.Count > 0)
        {
            command.CommandText += " WHERE " + string.Join(" AND ", whereClauses);
        }
        
        using var reader = await ((DbCommand)command).ExecuteReaderAsync(cancellationToken);
        await foreach (var entity in MapToProductoEntityAsync(reader, cancellationToken))
        {
            yield return entity.ToDomain();
        }
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Producto (Nombre, SKU, Marca, Precio, Stock)
            VALUES (@Nombre, @SKU, @Marca, @Precio, @Stock);
            SELECT SCOPE_IDENTITY();"; 

        command.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.NVarChar) { Value = product.Name });
        command.Parameters.Add(new SqlParameter("@SKU", SqlDbType.VarChar) { Value = product.SKU });
        command.Parameters.Add(new SqlParameter("@Marca", SqlDbType.NVarChar) { Value = product.Brand ?? (object)DBNull.Value });
        command.Parameters.Add(new SqlParameter("@Precio", SqlDbType.Decimal) { Value = product.Price });
        command.Parameters.Add(new SqlParameter("@Stock", SqlDbType.Int) { Value = product.Stock });

        var newId = Convert.ToInt32(await ((DbCommand)command).ExecuteScalarAsync(cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve new product ID."));
        
        // Create a new Product instance with the generated ID
        return product with { ProductID = newId };
    }

    public async Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Producto
            SET Nombre = @Nombre,
                SKU = @SKU,
                Marca = @Marca,
                Precio = @Precio,
                Stock = @Stock
            WHERE ProductoID = @ProductId";

        command.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.NVarChar) { Value = product.Name });
        command.Parameters.Add(new SqlParameter("@SKU", SqlDbType.VarChar) { Value = product.SKU });
        command.Parameters.Add(new SqlParameter("@Marca", SqlDbType.NVarChar) { Value = product.Brand ?? (object)DBNull.Value });
        command.Parameters.Add(new SqlParameter("@Precio", SqlDbType.Decimal) { Value = product.Price });
        command.Parameters.Add(new SqlParameter("@Stock", SqlDbType.Int) { Value = product.Stock });
        command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = product.ProductID });

        var rowsAffected = await ((DbCommand)command).ExecuteNonQueryAsync(cancellationToken);
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateStockAsync(int productId, int newStock, CancellationToken cancellationToken = default)
    {
        if (newStock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(newStock), "New stock cannot be negative.");
        }

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Producto SET Stock = @NewStock WHERE ProductoID = @ProductId";
        command.Parameters.Add(new SqlParameter("@NewStock", SqlDbType.Int) { Value = newStock });
        command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });

        var rowsAffected = await ((DbCommand)command).ExecuteNonQueryAsync(cancellationToken);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int productId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Producto WHERE ProductoID = @ProductId";
        command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });

        var rowsAffected = await ((DbCommand)command).ExecuteNonQueryAsync(cancellationToken);
        return rowsAffected > 0;
    }
}
