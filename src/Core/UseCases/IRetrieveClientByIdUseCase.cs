// src/Core/UseCases/IRetrieveClientByIdUseCase.cs

using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;

namespace Utm_market.Core.UseCases;

/// <summary>
/// Define el contrato para el caso de uso que recupera un cliente por su identificador único.
/// </summary>
public interface IRetrieveClientByIdUseCase
{
    /// <summary>
    /// Ejecuta el caso de uso para obtener un cliente específico.
    /// </summary>
    /// <param name="clientId">El identificador único del cliente a recuperar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una <see cref="Task{Client}"/> que contendrá el cliente encontrado, o null si no existe.</returns>
    Task<Client?> ExecuteAsync(int clientId, CancellationToken cancellationToken = default);
}
