# Diseño e Implementación de Mappers para Transacciones (Venta y Detalles)

Este documento detalla el diseño e implementación de la clase `SaleMapper` para el proyecto "Utm_market", enfocándose en la transformación profunda y bidireccional de objetos transaccionales (`Sale` y `SaleDetail`) y sus representaciones de persistencia (`VentaEntity` y `DetalleVentaEntity`). Se hace uso extensivo de las capacidades de C# 14 y `extension blocks` para asegurar compatibilidad con Native AOT y un rendimiento óptimo.

## 1. Árbol de Directorios de la Capa de Infraestructura

A continuación, se presenta la estructura de directorios de la capa `src/Infrastructure`, destacando la organización de los mappers y los modelos de datos.

```
src\Infrastructure
├───Mappers
│   ├───ProductMapper.cs
│      └───SaleMapper.cs
└───Models
    └───Data
        ├───DetalleVentaEntity.cs
        ├───ProductoEntity.cs
        └───VentaEntity.cs
```

## 2. Código Fuente Completo de `SaleMapper.cs`

Esta clase estática contiene métodos de extensión diseñados para un mapeo profundo, gestionando no solo la entidad principal de venta sino también su colección de detalles asociados. Se garantiza la compatibilidad con Native AOT al evitar el uso de reflexión.

```csharp
// src/Infrastructure/Mappers/SaleMapper.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UtmMarketCli.Core.Entities;
using UtmMarketCli.Infrastructure.Models.Data;

namespace UtmMarketCli.Infrastructure.Mappers;

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
            Cantidad = domain.Cantidad,
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
            .Select(d => d.ToEntity(sale.SaleID)) // Pass sale.SaleID here, it will be updated after insert if new sale
            .ToList();

        return (ventaEntity, detalleEntities);
    }
}
```

## 3. Ejemplo de Uso dentro de un Repositorio

El siguiente snippet ilustra cómo un repositorio de ventas podría utilizar el `SaleMapper` para gestionar la recuperación y persistencia de ventas completas, incluyendo sus detalles. Este ejemplo asume el uso de Dapper para la interacción con la base de datos.

```csharp
// src/Infrastructure/Data/SaleRepository.cs (Hypothetical)
using System.Data.SqlClient;
using Dapper;
using UtmMarketCli.Core.Entities;
using UtmMarketCli.Infrastructure.Models.Data;
using UtmMarketCli.Infrastructure.Mappers; // Importar para usar los métodos de extensión
using Microsoft.Extensions.Configuration;

namespace UtmMarketCli.Infrastructure.Data;

public class SaleRepository
{
    private readonly string _connectionString;

    public SaleRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("DefaultConnection not configured.");
    }

    /// <summary>
    /// Obtiene una venta completa por su ID, incluyendo sus detalles, y la convierte a la entidad de dominio.
    /// </summary>
    /// <param name="saleId">El ID de la venta.</param>
    /// <returns>El objeto de dominio <see cref="Sale"/> o null si no se encuentra.</returns>
    public async Task<Sale?> GetSaleByIdAsync(int saleId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            SELECT VentaID, Folio, FechaVenta, TotalArticulos, TotalVenta, Estatus
            FROM Venta WHERE VentaID = @SaleId;

            SELECT DetalleID, VentaID, ProductoID, PrecioUnitario, Cantidad, TotalDetalle
            FROM DetalleVenta WHERE VentaID = @SaleId;";

        // Usando QueryMultiple de Dapper para obtener la entidad principal y sus hijos
        using (var multi = await connection.QueryMultipleAsync(sql, new { SaleId = saleId }))
        {
            var ventaEntity = await multi.ReadFirstOrDefaultAsync<VentaEntity>();
            var detalleEntities = (await multi.ReadAsync<DetalleVentaEntity>()).ToList();

            if (ventaEntity == null)
            {
                return null;
            }

            // Usar el mapeador para convertir la entidad de persistencia y sus detalles a la entidad de dominio
            return ventaEntity.ToDomain(detalleEntities);
        }
    }

    /// <summary>
    /// Añade una nueva venta con sus detalles a la base de datos.
    /// </summary>
    /// <param name="sale">El objeto de dominio <see cref="Sale"/> a añadir.</param>
    public async Task AddSaleAsync(Sale sale)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = connection.BeginTransaction();

        try
        {
            // Convertir la entidad de dominio a la entidad de persistencia y sus detalles
            var (ventaEntity, initialDetalleEntities) = sale.ToEntity();

            // Insertar la venta principal
            var insertVentaSql = @"
                INSERT INTO Venta (Folio, FechaVenta, TotalArticulos, TotalVenta, Estatus)
                VALUES (@Folio, @FechaVenta, @TotalArticulos, @TotalVenta, @Estatus);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            var newVentaId = await connection.ExecuteScalarAsync<int>(insertVentaSql, ventaEntity, transaction);

            // Re-mapear los detalles con el nuevo VentaID para la inserción
            var detailsForInsert = sale.SaleDetails
                .Select(d => d.ToEntity(newVentaId))
                .ToList();

            // Insertar los detalles de la venta
            var insertDetalleVentaSql = @"
                INSERT INTO DetalleVenta (VentaID, ProductoID, PrecioUnitario, Cantidad, TotalDetalle)
                VALUES (@VentaID, @ProductoID, @PrecioUnitario, @Cantidad, @TotalDetalle);";
            await connection.ExecuteAsync(insertDetalleVentaSql, detailsForInsert, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
```

## 4. Nota de Arquitectura: Beneficios de C# 14 Extension Members en este Escenario

El uso de 'extension blocks' (métodos de extensión) de C# 14 en la implementación de `SaleMapper` ofrece ventajas cruciales, especialmente en el contexto de un mapeo profundo para transacciones como las ventas y sus detalles, y en un entorno optimizado para Native AOT como .NET 10.

1.  **Mapeo Profundo Ergónomico:** Para objetos complejos con colecciones anidadas, como `Sale` con `SaleDetails`, los métodos de extensión permiten una sintaxis de mapeo muy limpia y fluida. En lugar de pasar múltiples argumentos a un método estático o construir un constructor complicado, la capacidad de encadenar `.ToDomain()` o `.ToEntity()` directamente en los objetos hace que el flujo de transformación sea intuitivo y fácil de seguir. La orquestación del mapeo de colecciones (como `detalleEntities.Select(d => d.ToDomain())`) se integra perfectamente.

2.  **Rendimiento y Compatibilidad con Native AOT:**
    *   **Cero Reflexión:** Los métodos de extensión son resoluciones en tiempo de compilación. No hay uso de reflexión en tiempo de ejecución, lo cual es fundamental para la compatibilidad con Native AOT. Esto significa que el `trimming` del compilador puede ser agresivo, produciendo binarios más pequeños y de inicio más rápido, sin riesgo de que se elimine código esencial por depender de la reflexión.
    *   **Baja Sobrecarga:** Al ser estáticos, no requieren instanciación ni gestión de ciclo de vida por parte del contenedor de inyección de dependencias, reduciendo la sobrecarga de memoria y CPU. Esto es ideal para operaciones de mapeo de datos que pueden ocurrir con alta frecuencia.

3.  **Separación de Preocupaciones y Pureza del Dominio:** Los mappers residen en la capa de infraestructura, actuando como un puente entre el dominio puro y la capa de persistencia. Esto mantiene las entidades de dominio limpias y sin dependencias de la infraestructura, lo cual es un pilar de la Clean Architecture. Las entidades no "conocen" cómo se persisten, solo su propia lógica de negocio.

4.  **Coherencia y Reutilización:** Una vez definidos, estos métodos de extensión pueden ser reutilizados fácilmente en cualquier parte de la capa de infraestructura que necesite realizar transformaciones entre estos tipos. Esto promueve la coherencia en el mapeo de datos y reduce la probabilidad de errores.

5.  **Manejo de la Integridad del Estado Transaccional:** Como se vio en el ejemplo del repositorio, el mapeo de `Sale` a `VentaEntity` puede implicar ajustar `VentaID` en los `DetalleVentaEntity` después de que la venta principal ha sido insertada y su ID generada por la base de datos. Los métodos de extensión, junto con la construcción de tuplas o clases de transporte, permiten gestionar este flujo de trabajo transaccional de manera controlada y explícita.

En resumen, los 'extension blocks' de C# 14 son una herramienta poderosa que permite implementar una capa de mapeo de datos eficiente, legible, y totalmente compatible con las exigencias de rendimiento y despliegue de aplicaciones Native AOT en .NET 10, manteniendo la integridad arquitectónica del proyecto.
