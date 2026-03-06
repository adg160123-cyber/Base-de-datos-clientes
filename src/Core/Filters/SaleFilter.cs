// src/Core/Filters/SaleFilter.cs
using System;
using Utm_market.Core.Entities;

namespace Utm_market.Core.Filters;

/// <summary>
/// Representa los criterios de filtro para buscar ventas.
/// Utiliza un record inmutable para concisión y seguridad.
/// </summary>
public record SaleFilter
{
    /// <summary>
    /// Folio de la venta a buscar (opcional, búsqueda exacta).
    /// </summary>
    public string? Folio { get; init; }

    /// <summary>
    /// Estatus de la venta a buscar (opcional).
    /// </summary>
    public SaleStatus? Status { get; init; }

    /// <summary>
    /// Fecha de venta inicial (inclusive) para el rango de búsqueda (opcional).
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Fecha de venta final (inclusive) para el rango de búsqueda (opcional).
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// ID de producto para buscar ventas que contengan ese producto (opcional).
    /// </summary>
    public int? ProductIdInDetails { get; init; }
}
