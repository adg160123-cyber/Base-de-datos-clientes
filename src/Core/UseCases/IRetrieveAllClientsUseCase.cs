// src/Core/UseCases/IRetrieveAllClientsUseCase.cs

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;

namespace Utm_market.Core.UseCases;

/// <summary>
/// Define el contrato para el caso de uso que recupera todos los clientes.
/// Permite el procesamiento en streaming para optimizar el uso de memoria.
/// </summary>
public interface IRetrieveAllClientsUseCase
{
    /// <summary>
    /// Ejecuta el caso de uso para obtener todos los clientes.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="IAsyncEnumerable{Client}"/> que representa un flujo de todos los clientes.</returns>
    IAsyncEnumerable<Client> ExecuteAsync(CancellationToken cancellationToken = default);
}
