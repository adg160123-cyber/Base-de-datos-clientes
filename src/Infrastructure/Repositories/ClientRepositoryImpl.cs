// src/Infrastructure/Repositories/ClientRepositoryImpl.cs

using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;
using Utm_market.Core.Filters;
using Utm_market.Core.Repositories;
using Utm_market.Infrastructure.Data;
using Utm_market.Infrastructure.Mappers;
using Utm_market.Infrastructure.Models.Data;
using System.Runtime.CompilerServices; // Add this line

namespace Utm_market.Infrastructure.Repositories;

/// <summary>
/// Implementación concreta de <see cref="IClientRepository"/> para SQL Server,
/// utilizando mapeo manual con <see cref="SqlDataReader"/> para compatibilidad
/// con Native AOT y alto rendimiento.
/// </summary>
public sealed class ClientRepositoryImpl : IClientRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ClientRepositoryImpl"/>.
    /// </summary>
    /// <param name="connectionFactory">Fábrica para crear conexiones a la base de datos.</param>
    public ClientRepositoryImpl(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<Client> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT ClienteID, Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion, FechaRegistro, Activo FROM Cliente";
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, (SqlConnection)connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return MapClienteEntityToClient(reader);
        }
    }

    /// <inheritdoc/>
    public async Task<Client?> GetByIdAsync(int clientId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT ClienteID, Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion, FechaRegistro, Activo FROM Cliente WHERE ClienteID = @ClientId";
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, (SqlConnection)connection);
        command.Parameters.AddWithValue("@ClientId", clientId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return MapClienteEntityToClient(reader);
        }
        return null;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<Client> FindAsync(ClientFilter filter, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sqlBuilder = new System.Text.StringBuilder("SELECT ClienteID, Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion, FechaRegistro, Activo FROM Cliente WHERE 1=1");
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sqlBuilder.ToString(), (SqlConnection)connection);

        if (!string.IsNullOrWhiteSpace(filter.Nombre))
        {
            sqlBuilder.Append(" AND Nombre LIKE @Nombre");
            command.Parameters.AddWithValue("@Nombre", $"%{filter.Nombre}%");
        }
        if (!string.IsNullOrWhiteSpace(filter.ApellidoPaterno))
        {
            sqlBuilder.Append(" AND ApellidoPaterno LIKE @ApellidoPaterno");
            command.Parameters.AddWithValue("@ApellidoPaterno", $"%{filter.ApellidoPaterno}%");
        }
        if (!string.IsNullOrWhiteSpace(filter.Email))
        {
            sqlBuilder.Append(" AND Email LIKE @Email");
            command.Parameters.AddWithValue("@Email", $"%{filter.Email}%");
        }
        if (filter.Activo.HasValue)
        {
            sqlBuilder.Append(" AND Activo = @Activo");
            command.Parameters.AddWithValue("@Activo", filter.Activo.Value);
        }

        command.CommandText = sqlBuilder.ToString();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return MapClienteEntityToClient(reader);
        }
    }

    /// <inheritdoc/>
    public async Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Cliente (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion, FechaRegistro, Activo)
            VALUES (@Nombre, @ApellidoPaterno, @ApellidoMaterno, @Telefono, @Email, @Direccion, @FechaRegistro, @Activo);
            SELECT SCOPE_IDENTITY();"; 

        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, (SqlConnection)connection);
        
        // Add parameters manually
        command.Parameters.AddWithValue("@Nombre", client.Nombre);
        command.Parameters.AddWithValue("@ApellidoPaterno", client.ApellidoPaterno);
        command.Parameters.AddWithValue("@ApellidoMaterno", (object?)client.ApellidoMaterno ?? DBNull.Value);
        command.Parameters.AddWithValue("@Telefono", (object?)client.Telefono ?? DBNull.Value);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Direccion", (object?)client.Direccion ?? DBNull.Value);
        command.Parameters.AddWithValue("@FechaRegistro", client.FechaRegistro);
        command.Parameters.AddWithValue("@Activo", client.Activo);

        var newId = (int)(decimal)await command.ExecuteScalarAsync(cancellationToken);

        // Create a new Client instance with the generated ID
        return new Client(
            newId,
            client.Nombre,
            client.ApellidoPaterno,
            client.Email,
            client.FechaRegistro,
            client.Activo,
            client.ApellidoMaterno,
            client.Telefono,
            client.Direccion
        );
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(Client client, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Cliente
            SET
                Nombre = @Nombre,
                ApellidoPaterno = @ApellidoPaterno,
                ApellidoMaterno = @ApellidoMaterno,
                Telefono = @Telefono,
                Email = @Email,
                Direccion = @Direccion,
                Activo = @Activo
            WHERE ClienteID = @ClienteID;";

        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, (SqlConnection)connection);
        
        // Add parameters manually
        command.Parameters.AddWithValue("@Nombre", client.Nombre);
        command.Parameters.AddWithValue("@ApellidoPaterno", client.ApellidoPaterno);
        command.Parameters.AddWithValue("@ApellidoMaterno", (object?)client.ApellidoMaterno ?? DBNull.Value);
        command.Parameters.AddWithValue("@Telefono", (object?)client.Telefono ?? DBNull.Value);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Direccion", (object?)client.Direccion ?? DBNull.Value);
        command.Parameters.AddWithValue("@Activo", client.Activo);
        command.Parameters.AddWithValue("@ClienteID", client.ClienteID);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int clientId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Cliente WHERE ClienteID = @ClientId";

        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, (SqlConnection)connection);
        command.Parameters.AddWithValue("@ClientId", clientId);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    /// <summary>
    /// Mapea una fila de <see cref="SqlDataReader"/> a una entidad de dominio <see cref="Client"/>.
    /// </summary>
    /// <param name="reader">El <see cref="SqlDataReader"/> actual.</param>
    /// <returns>Una entidad <see cref="Client"/>.</returns>
    private static Client MapClienteEntityToClient(SqlDataReader reader)
    {
        var clienteEntity = new ClienteEntity
        {
            ClienteID = reader.GetInt32(reader.GetOrdinal("ClienteID")),
            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
            ApellidoPaterno = reader.GetString(reader.GetOrdinal("ApellidoPaterno")),
            ApellidoMaterno = reader.IsDBNull(reader.GetOrdinal("ApellidoMaterno")) ? null : reader.GetString(reader.GetOrdinal("ApellidoMaterno")),
            Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString(reader.GetOrdinal("Telefono")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) ? null : reader.GetString(reader.GetOrdinal("Direccion")),
            FechaRegistro = reader.GetDateTime(reader.GetOrdinal("FechaRegistro")),
            Activo = reader.GetBoolean(reader.GetOrdinal("Activo"))
        };
        return clienteEntity.ToDomain();
    }
}
