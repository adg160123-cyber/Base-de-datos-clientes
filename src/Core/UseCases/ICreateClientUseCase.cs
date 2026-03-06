// src/Core/UseCases/ICreateClientUseCase.cs

using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;

namespace Utm_market.Core.UseCases;

/// <summary>
/// Define el contrato para el caso de uso que crea un nuevo cliente.
/// </summary>
public interface ICreateClientUseCase
{
    /// <summary>
    /// Ejecuta el caso de uso para crear un nuevo cliente.
    /// </summary>
    /// <param name="client">El objeto <see cref="Client"/> a crear.
    /// Su <see cref="Client.ClienteID"/> será actualizado con el ID generado por la persistencia.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una <see cref="Task{Client}"/> que contendrá el cliente creado con su ID asignado.</returns>
    Task<Client> ExecuteAsync(Client client, CancellationToken cancellationToken = default);
}
