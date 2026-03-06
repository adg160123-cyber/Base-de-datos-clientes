// src/Infrastructure/Data/IDbConnectionFactory.cs

using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Utm_market.Infrastructure.Data;

/// <summary>
/// Define un contrato para la creación de conexiones a bases de datos,
/// aislando a los consumidores de los detalles específicos de implementación
/// y facilitando la inyección de dependencias.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Crea y abre de forma asíncrona una nueva conexión a la base de datos.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una <see cref="IDbConnection"/> abierta.</returns>
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}
