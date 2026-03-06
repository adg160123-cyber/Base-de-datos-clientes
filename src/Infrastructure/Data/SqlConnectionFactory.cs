// src/Infrastructure/Data/SqlConnectionFactory.cs

using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Utm_market.Infrastructure.Data;

/// <summary>
/// Implementación de <see cref="IDbConnectionFactory"/> para SQL Server.
/// Gestiona la creación y apertura de conexiones <see cref="SqlConnection"/>
/// utilizando una cadena de conexión configurada.
/// </summary>
public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="SqlConnectionFactory"/>.
    /// </summary>
    /// <param name="configuration">Instancia de <see cref="IConfiguration"/> para acceder a la cadena de conexión.</param>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="configuration"/> es nulo.</exception>
    /// <exception cref="InvalidOperationException">Se lanza si la cadena de conexión "DefaultConnection" no está configurada o es inválida.</exception>
    public SqlConnectionFactory(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        // La palabra clave 'field' aplica a los accessors de propiedades.
        // Para un campo readonly asignado en el constructor, la validación se realiza directamente.
        // Se asegura que la cadena de conexión sea válida antes de su asignación.
        var connString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connString))
        {
            throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada o es inválida.");
        }
        _connectionString = connString;
    }

    /// <summary>
    /// Crea y abre de forma asíncrona una nueva conexión a SQL Server.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una <see cref="SqlConnection"/> abierta.</returns>
    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
