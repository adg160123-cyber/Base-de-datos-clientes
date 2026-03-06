// src/Infrastructure/Mappers/SaleMapper.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Utm_market.Core.Entities;
using Utm_market.Infrastructure.Models.Data;

namespace Utm_market.Infrastructure.Mappers;

/// <summary>
/// Proporciona métodos de extensión estáticos para la conversión bidireccional
/// entre las entidades de dominio (<see cref="Sale"/>, <see cref="SaleDetail"/>)
/// y las entidades de persistencia (<see cref="VentaEntity"/>, <see cref="DetalleVentaEntity"/>).
/// Diseñado para ser compatible con Native AOT y C# 14 'extension blocks', permitiendo un mapeo profundo.
/// </summary>
public static class SaleMapper
{
    // =======================================================================
    // Mapeo de Estatus de Venta (byte <-> SaleStatus)
    // =======================================================================

    /// <summary>
    /// Convierte un valor byte de estatus de venta (desde la base de datos) a su correspondiente <see cref="SaleStatus"/> de dominio.
    /// </summary>
    /// <param name="estatusByte">El valor byte del estatus.</param>
    /// <returns>El <see cref="SaleStatus"/> correspondiente.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Se lanza si el valor byte no corresponde a un estatus válido.</exception>
    public static SaleStatus ToDomainSaleStatus(this byte estatusByte) => estatusByte switch
    {
        1 => SaleStatus.Pending,
        2 => SaleStatus.Completed,
        3 => SaleStatus.Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(estatusByte), $"Invalid Estatus byte value: {estatusByte}. Expected 1, 2, or 3.")
    };

    /// <summary>
    /// Convierte un <see cref="SaleStatus"/> de dominio a su correspondiente valor byte para la base de datos.
    /// </summary>
    /// <param name="saleStatus">El <see cref="SaleStatus"/> a convertir.</param>
    /// <returns>El valor byte correspondiente.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Se lanza si el <see cref="SaleStatus"/> no es mapeable a la base de datos.</exception>
    public static byte ToEntityEstatusByte(this SaleStatus saleStatus) => saleStatus switch
    {
        SaleStatus.Pending => 1,
        SaleStatus.Completed => 2,
        SaleStatus.Cancelled => 3,
        // SQL CHECK constraint only allows 1, 2, 3. AwaitingPayment (4) is not directly supported by DB schema.
        SaleStatus.AwaitingPayment => throw new ArgumentOutOfRangeException(nameof(saleStatus), $"SaleStatus '{saleStatus}' cannot be mapped to VentaEntity.Estatus as it's not supported by the database schema (1, 2, 3 only)."),
        _ => throw new ArgumentOutOfRangeException(nameof(saleStatus), $"Unhandled SaleStatus value: {saleStatus}.")
    };

    // =======================================================================
    // Mapeo de Detalle de Venta (DetalleVentaEntity <-> SaleDetail)
    // =======================================================================

    /// <summary>
    /// Convierte una entidad de persistencia <see cref="DetalleVentaEntity"/> a una entidad de dominio <see cref="SaleDetail"/>.
    /// </summary>
    /// <param name="entity">La instancia de <see cref="DetalleVentaEntity"/> a convertir.</param>
    /// <returns>Una nueva instancia de <see cref="SaleDetail"/>.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si la entidad de entrada es nula.</exception>
    public static SaleDetail ToDomain(this DetalleVentaEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new SaleDetail(
            saleDetailId: entity.DetalleID,
            productId: entity.ProductoID,
            quantity: entity.Cantidad,
            unitPrice: entity.PrecioUnitario
        );
    }

    /// <summary>
    /// Convierte una entidad de dominio <see cref="SaleDetail"/> a una entidad de persistencia <see cref="DetalleVentaEntity"/>.
    /// </summary>
    /// <param name="domain">La instancia de <see cref="SaleDetail"/> a convertir.</param>
    /// <param name="ventaId">El ID de la venta a la que pertenece este detalle (necesario para <see cref="DetalleVentaEntity"/>).</param>
    /// <returns>Una nueva instancia de <see cref="DetalleVentaEntity"/>.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si la entidad de dominio de entrada es nula.</exception>
    public static DetalleVentaEntity ToEntity(this SaleDetail domain, int ventaId)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new DetalleVentaEntity(
            detalleId: domain.SaleDetailID,
            ventaId: ventaId, // Se asigna el ID de la Venta aquí
            productoId: domain.ProductID)
        {
            PrecioUnitario = domain.UnitPrice,
            Cantidad = domain.Quantity,
            TotalDetalle = domain.TotalDetail
        };
    }

    // =======================================================================
    // Mapeo de Venta (VentaEntity <-> Sale) - Mapeo Profundo
    // =======================================================================

    /// <summary>
    /// Convierte una entidad de persistencia <see cref="VentaEntity"/> y su colección de detalles
    /// a una entidad de dominio <see cref="Sale"/>.
    /// </summary>
    /// <param name="ventaEntity">La instancia de <see cref="VentaEntity"/> a convertir.</param>
    /// <param name="detalleEntities">La colección de <see cref="DetalleVentaEntity"/> asociadas a la venta.</param>
    /// <returns>Una nueva instancia de <see cref="Sale"/>.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si la entidad de venta o la colección de detalles es nula.</exception>
    public static Sale ToDomain(this VentaEntity ventaEntity, IEnumerable<DetalleVentaEntity> detalleEntities)
    {
        ArgumentNullException.ThrowIfNull(ventaEntity);
        ArgumentNullException.ThrowIfNull(detalleEntities);

        var saleDetails = detalleEntities.Select(d => d.ToDomain()).ToList();

        return new Sale(
            saleId: ventaEntity.VentaID,
            folio: ventaEntity.Folio,
            status: ventaEntity.Estatus.ToDomainSaleStatus()) // Mapeo de estatus
        {
            SaleDate = ventaEntity.FechaVenta,
            SaleDetails = saleDetails // Asigna la colección de detalles mapeados
        };
    }

    /// <summary>
    /// Convierte una entidad de dominio <see cref="Sale"/> a una entidad de persistencia <see cref="VentaEntity"/>
    /// y una colección de <see cref="DetalleVentaEntity"/>.
    /// </summary>
    /// <param name="sale">La instancia de <see cref="Sale"/> a convertir.</param>
    /// <returns>Una tupla que contiene la <see cref="VentaEntity"/> y una colección de <see cref="DetalleVentaEntity"/>.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si la entidad de dominio de entrada es nula.</exception>
    public static (VentaEntity Venta, IEnumerable<DetalleVentaEntity> Detalles) ToEntity(this Sale sale)
    {
        ArgumentNullException.ThrowIfNull(sale);

        // Mapea la entidad principal de Venta
        var ventaEntity = new VentaEntity(
            ventaId: sale.SaleID,
            folio: sale.Folio)
        {
            FechaVenta = sale.SaleDate,
            TotalArticulos = sale.TotalItems, // Se usan las propiedades calculadas del dominio
            TotalVenta = sale.TotalSale,       // Se usan las propiedades calculadas del dominio
            Estatus = sale.Status.ToEntityEstatusByte() // Mapeo de estatus
        };

        // Mapea la colección de detalles, pasando el VentaID (que puede ser 0 si es una nueva venta)
        var detalleEntities = sale.SaleDetails
            .Select(d => d.ToEntity(sale.SaleID))
            .ToList();

        return (ventaEntity, detalleEntities);
    }
}
