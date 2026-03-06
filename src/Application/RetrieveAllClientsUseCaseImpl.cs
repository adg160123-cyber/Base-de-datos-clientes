// src/Application/RetrieveAllClientsUseCaseImpl.cs

using System.Collections.Generic;
using System.Threading;
using Utm_market.Core.Entities;
using Utm_market.Core.Repositories;
using Utm_market.Core.UseCases;

namespace Utm_market.Application;

/// <summary>
/// Implementación del caso de uso para recuperar todos los clientes.
/// </summary>
public sealed class RetrieveAllClientsUseCaseImpl : IRetrieveAllClientsUseCase
{
    private readonly IClientRepository _clientRepository;

    /// <summary>
    /// Constructor primario para inyección de dependencias.
    /// </summary>
    /// <param name="clientRepository">El repositorio de clientes.</param>
    public RetrieveAllClientsUseCaseImpl(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<Client> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return _clientRepository.GetAllAsync(cancellationToken);
    }
}
