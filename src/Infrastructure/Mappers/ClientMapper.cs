// src/Infrastructure/Mappers/ClientMapper.cs

using Utm_market.Core.Entities;
using Utm_market.Infrastructure.Models.Data;
using System;

namespace Utm_market.Infrastructure.Mappers;

/// <summary>
/// Proporciona métodos de extensión estáticos para la conversión bidireccional
/// entre la entidad de dominio <see cref="Client"/> y la entidad de persistencia
/// <see cref="ClienteEntity"/>. Estos métodos están diseñados para ser eficientes
/// y compatibles con Native AOT en .NET 10.
/// </summary>
public static class ClientMapper
{
    /// <summary>
    /// Convierte un objeto <see cref="ClienteEntity"/> (capa de infraestructura)
    /// a un objeto <see cref="Client"/> (capa de dominio).
    /// </summary>
    /// <param name="clienteEntity">La entidad de persistencia a convertir.</param>
    /// <returns>Una nueva instancia de la entidad de dominio <see cref="Client"/>.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="clienteEntity"/> es nulo.</exception>
    public static Client ToDomain(this ClienteEntity clienteEntity)
    {
        ArgumentNullException.ThrowIfNull(clienteEntity);

        return new Client(
            clienteEntity.ClienteID,
            clienteEntity.Nombre,
            clienteEntity.ApellidoPaterno,
            clienteEntity.Email,
            clienteEntity.FechaRegistro,
            clienteEntity.Activo,
            clienteEntity.ApellidoMaterno,
            clienteEntity.Telefono,
            clienteEntity.Direccion
        );
    }

    /// <summary>
    /// Convierte un objeto <see cref="Client"/> (capa de dominio)
    /// a un objeto <see cref="ClienteEntity"/> (capa de infraestructura).
    /// </summary>
    /// <param name="client">La entidad de dominio a convertir.</param>
    /// <returns>Una nueva instancia de la entidad de persistencia <see cref="ClienteEntity"/>.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="client"/> es nulo.</exception>
    public static ClienteEntity ToEntity(this Client client)
    {
        ArgumentNullException.ThrowIfNull(client);

        return new ClienteEntity
        {
            ClienteID = client.ClienteID,
            Nombre = client.Nombre,
            ApellidoPaterno = client.ApellidoPaterno,
            ApellidoMaterno = client.ApellidoMaterno,
            Telefono = client.Telefono,
            Email = client.Email,
            Direccion = client.Direccion,
            FechaRegistro = client.FechaRegistro,
            Activo = client.Activo
        };
    }
}
