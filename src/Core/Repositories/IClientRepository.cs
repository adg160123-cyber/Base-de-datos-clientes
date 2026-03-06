// src/Core/Repositories/IClientRepository.cs

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;
using Utm_market.Core.Filters;

namespace Utm_market.Core.Repositories;

/// <summary>
/// Define el contrato para el repositorio de clientes, aislando la capa de dominio
/// de los detalles de persistencia. Esta interfaz trabaja exclusivamente con la entidad
/// de dominio <see cref="Client"/> y está diseñada para ser compatible con Native AOT.
/// </summary>
public interface IClientRepository
{
    /// <summary>
    /// Recupera de forma asíncrona todos los clientes, permitiendo el procesamiento
    /// en streaming para optimizar el uso de memoria en grandes conjuntos de datos.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="IAsyncEnumerable{T}"/> de clientes.</returns>
    IAsyncEnumerable<Client> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera de forma asíncrona un cliente por su identificador único.
    /// </summary>
    /// <param name="clientId">El identificador único del cliente.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una <see cref="Task{TResult}"/> que representa la operación asíncrona,
    /// conteniendo el <see cref="Client"/> encontrado o null si no existe.</returns>
    Task<Client?> GetByIdAsync(int clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca clientes de forma asíncrona aplicando criterios de filtro.
    /// </summary>
    /// <param name="filter">Un objeto <see cref="ClientFilter"/> que contiene los criterios de búsqueda.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="IAsyncEnumerable{T}"/> de clientes que coinciden con el filtro.</returns>
    IAsyncEnumerable<Client> FindAsync(ClientFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega un nuevo cliente de forma asíncrona al repositorio y devuelve la entidad
    /// con su identificador generado por la base de datos.
    /// </summary>
    /// <param name="client">La entidad <see cref="Client"/> a agregar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una <see cref="Task{TResult}"/> que representa la operación asíncrona,
    /// conteniendo la entidad <see cref="Client"/> con su ID generado.</returns>
    Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un cliente existente de forma asíncrona en el repositorio.
    /// </summary>
    /// <param name="client">La entidad <see cref="Client"/> con los datos actualizados.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una <see cref="Task{TResult}"/> que representa la operación asíncrona,
    /// conteniendo un booleano que indica si la actualización fue exitosa.</returns>
    Task<bool> UpdateAsync(Client client, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un cliente existente de forma asíncrona del repositorio.
    /// </summary>
    /// <param name="clientId">El identificador único del cliente a eliminar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una <see cref="Task{TResult}"/> que representa la operación asíncrona,
    /// conteniendo un booleano que indica si la eliminación fue exitosa.</returns>
    Task<bool> DeleteAsync(int clientId, CancellationToken cancellationToken = default);
}
