// src/Infrastructure/Models/Data/ClienteEntity.cs

using System;

namespace Utm_market.Infrastructure.Models.Data;

/// <summary>
/// Representa la entidad de Cliente a nivel de persistencia de datos.
/// Esta clase es un POCO (Plain Old CLR Object) utilizado por Dapper
/// para mapear directamente a las columnas de la tabla [dbo].[Cliente].
/// Está diseñada para ser compatible con Native AOT y no contiene lógica de dominio.
/// </summary>
public sealed class ClienteEntity
{
    /// <summary>
    /// Identificador único del cliente (Primary Key).
    /// </summary>
    public int ClienteID { get; set; }

    /// <summary>
    /// Nombre del cliente.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Apellido paterno del cliente.
    /// </summary>
    public string ApellidoPaterno { get; set; } = string.Empty;

    /// <summary>
    /// Apellido materno del cliente. Puede ser nulo.
    /// </summary>
    public string? ApellidoMaterno { get; set; }

    /// <summary>
    /// Número de teléfono del cliente. Puede ser nulo.
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Correo electrónico del cliente.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Dirección del cliente. Puede ser nula.
    /// </summary>
    public string? Direccion { get; set; }

    /// <summary>
    /// Fecha de registro del cliente.
    /// </summary>
    public DateTime FechaRegistro { get; set; }

    /// <summary>
    /// Indica si el cliente está activo.
    /// </summary>
    public bool Activo { get; set; }
}
