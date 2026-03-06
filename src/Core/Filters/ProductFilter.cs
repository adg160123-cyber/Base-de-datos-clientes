// src/Core/Filters/ProductFilter.cs
using System;

namespace Utm_market.Core.Filters;

/// <summary>
/// Representa los criterios de filtro para buscar productos.
/// Utiliza un record para inmutabilidad y concisión.
/// </summary>
public record ProductFilter
{
    /// <summary>
    /// Parte del nombre del producto a buscar (opcional).
    /// </summary>
    public string? NameContains { get; init; }

    /// <summary>
    /// SKU exacto del producto a buscar (opcional).
    /// </summary>
    public string? SKU { get; init; }

    /// <summary>
    /// Marca del producto a buscar (opcional).
    /// </summary>
    public string? Brand { get; init; }

    /// <summary>
    /// Precio mínimo del producto a buscar (opcional).
    /// </summary>
    public decimal? MinPrice { get; init; }

    /// <summary>
    /// Precio máximo del producto a buscar (opcional).
    /// </summary>
    public decimal? MaxPrice { get; init; }

    /// <summary>
    /// Stock mínimo del producto a buscar (opcional).
    /// </summary>
    public int? MinStock { get; init; }

    /// <summary>
    /// Stock máximo del producto a buscar (opcional).
    /// </summary>
    public int? MaxStock { get; init; }
}
