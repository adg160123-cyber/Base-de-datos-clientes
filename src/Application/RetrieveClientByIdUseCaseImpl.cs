// src/Application/RetrieveClientByIdUseCaseImpl.cs

using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;
using Utm_market.Core.Repositories;
using Utm_market.Core.UseCases;

namespace Utm_market.Application;

/// <summary>
/// Implementación del caso de uso para recuperar un cliente por su identificador único.
/// </summary>
public sealed class RetrieveClientByIdUseCaseImpl : IRetrieveClientByIdUseCase
{
    private readonly IClientRepository _clientRepository;

    /// <summary>
    /// Constructor primario para inyección de dependencias.
    /// </summary>
    /// <param name="clientRepository">El repositorio de clientes.</param>
    public RetrieveClientByIdUseCaseImpl(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    /// <inheritdoc/>
    public Task<Client?> ExecuteAsync(int clientId, CancellationToken cancellationToken = default)
    {
        return _clientRepository.GetByIdAsync(clientId, cancellationToken);
    }
}
