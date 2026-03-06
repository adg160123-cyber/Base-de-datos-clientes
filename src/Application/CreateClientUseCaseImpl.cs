// src/Application/CreateClientUseCaseImpl.cs

using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;
using Utm_market.Core.Repositories;
using Utm_market.Core.UseCases;

namespace Utm_market.Application;

/// <summary>
/// Implementación del caso de uso para crear un nuevo cliente.
/// </summary>
public sealed class CreateClientUseCaseImpl : ICreateClientUseCase
{
    private readonly IClientRepository _clientRepository;

    /// <summary>
    /// Constructor primario para inyección de dependencias.
    /// </summary>
    /// <param name="clientRepository">El repositorio de clientes.</param>
    public CreateClientUseCaseImpl(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    /// <inheritdoc/>
    public Task<Client> ExecuteAsync(Client client, CancellationToken cancellationToken = default)
    {
        // Se asume que el objeto Client recibido ya contiene las validaciones de dominio internas
        // y que la lógica de negocio necesaria ya se ha aplicado.
        return _clientRepository.AddAsync(client, cancellationToken);
    }
}
