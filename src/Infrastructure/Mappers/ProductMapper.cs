// src/Infrastructure/Mappers/ProductMapper.cs
using Utm_market.Core.Entities;
using Utm_market.Infrastructure.Models.Data;
using System;

namespace Utm_market.Infrastructure.Mappers;

/// <summary>
/// Proporciona métodos de extensión estáticos para la conversión bidireccional
/// entre la entidad de dominio <see cref="Product"/> y la entidad de persistencia <see cref="ProductoEntity"/>.
/// Diseñado para ser compatible con Native AOT y C# 14 'extension blocks'.
/// </summary>
public static class ProductMapper
{
    /// <summary>
    /// Convierte una entidad de persistencia <see cref="ProductoEntity"/> a una entidad de dominio <see cref="Product"/>.
    /// </summary>
    /// <param name="entity">La instancia de <see cref="ProductoEntity"/> a convertir.</param>
    /// <returns>Una nueva instancia de <see cref="Product"/>.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si la entidad de entrada es nula.</exception>
    public static Product ToDomain(this ProductoEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new Product(
            productId: entity.ProductoID,
            name: entity.Nombre,
            sku: entity.SKU,
            brand: entity.Marca ?? string.Empty,
            price: entity.Precio,
            stock: entity.Stock
        );
    }

    /// <summary>
    /// Convierte una entidad de dominio <see cref="Product"/> a una entidad de persistencia <see cref="ProductoEntity"/>.
    /// </summary>
    /// <param name="domain">La instancia de <see cref="Product"/> a convertir.</param>
    /// <returns>Una nueva instancia de <see cref="ProductoEntity"/>.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si la entidad de dominio de entrada es nula.</exception>
    public static ProductoEntity ToEntity(this Product domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new ProductoEntity(
            productoId: domain.ProductID,
            nombre: domain.Name,
            sku: domain.SKU)
        {
            Marca = domain.Brand, // Marca is nullable in entity, Brand is nullable in domain (after review of domain entity constructor)
            Precio = domain.Price,
            Stock = domain.Stock
        };
    }
}
