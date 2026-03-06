# Diseño del Contrato de Repositorio para la Raíz de Agregado "Sale"

Este documento define el contrato (`ISaleRepository`) para la gestión de la raíz de agregado `Sale` en el proyecto "Utm_market". Esta interfaz se ubica en la capa Core de la arquitectura limpia, asegurando que el dominio interactúe con la persistencia a través de una abstracción pura que maneja objetos de dominio (`Sale` y sus `SaleDetail`), y que está diseñada para ser compatible con Native AOT en .NET 10.

## 1. Código Fuente Completo de `SaleFilter.cs`

`SaleFilter` es una clase record inmutable que define los criterios de búsqueda para la entidad `Sale`.

```csharp
// src/Core/Filters/SaleFilter.cs
using System;
using UtmMarketCli.Core.Entities; // Necesario para el enum SaleStatus

namespace UtmMarketCli.Core.Filters;

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
```

## 2. Código Fuente Completo de `ISaleRepository.cs`

Esta interfaz define las operaciones CRUD y de consulta para la raíz de agregado `Sale`, incluyendo la carga de sus `SaleDetail`, asegurando la inyección de dependencias y la compatibilidad con operaciones asíncronas y eficientes en memoria.

```csharp
// src/Core/Repositories/ISaleRepository.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UtmMarketCli.Core.Entities;
using UtmMarketCli.Core.Filters; // Para SaleFilter

namespace UtmMarketCli.Core.Repositories;

/// <summary>
/// Define el contrato para el repositorio de ventas como raíz de agregado.
/// Esta interfaz es parte de la capa de Core (Dominio) y solo maneja objetos de dominio (<see cref="Sale"/>)
/// con sus colecciones de <see cref="SaleDetail"/> cargadas.
/// </summary>
public interface ISaleRepository
{
    /// <summary>
    /// Recupera todas las ventas de forma asíncrona como un flujo de datos.
    /// Cada <see cref="Sale"/> retornada incluye sus <see cref="SaleDetail"/> cargados.
    /// Utiliza <see cref="IAsyncEnumerable{T}"/> para un manejo eficiente de la memoria
    /// al permitir el streaming de resultados, ideal para .NET 10 y Native AOT.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="IAsyncEnumerable{Sale}"/> que representa un flujo de ventas.</returns>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    IAsyncEnumerable<Sale> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera una venta por su identificador único de forma asíncrona.
    /// La venta retornada incluye sus <see cref="SaleDetail"/> cargados.
    /// </summary>
    /// <param name="saleId">El identificador único de la venta.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="Task{Sale}"/> que contendrá la venta encontrada, o null si no existe.</returns>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    Task<Sale?> GetByIdAsync(int saleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca ventas que coincidan con los criterios especificados en el filtro.
    /// Cada <see cref="Sale"/> retornada incluye sus <see cref="SaleDetail"/> cargados.
    /// Este método es compatible con Native AOT al evitar expresiones genéricas.
    /// </summary>
    /// <param name="filter">Objeto <see cref="SaleFilter"/> con los criterios de búsqueda.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="IAsyncEnumerable{Sale}"/> que representa un flujo de ventas que coinciden con el filtro.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si el filtro es nulo.</exception>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    IAsyncEnumerable<Sale> FindAsync(SaleFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade una nueva venta (agregado completo con sus detalles) al repositorio de forma asíncrona.
    /// </summary>
    /// <param name="sale">El objeto <see cref="Sale"/> a añadir. Su <see cref="Sale.SaleID"/> será actualizado
    /// si la base de datos genera un nuevo ID.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="Task{Sale}"/> que representa la operación asíncrona, retornando la venta persistida
    /// con su identidad generada por la base de datos.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si la venta es nula.</exception>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    Task<Sale> AddAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza una venta existente (agregado completo con sus detalles) en el repositorio de forma asíncrona.
    /// </summary>
    /// <param name="sale">El objeto <see cref="Sale"/> con los datos actualizados, incluyendo sus detalles.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="Task{bool}"/> que representa la operación asíncrona.
    /// Retorna <c>true</c> si la venta fue encontrada y actualizada, <c>false</c> en caso contrario.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si la venta es nula.</exception>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    Task<bool> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una venta del repositorio de forma asíncrona.
    /// </summary>
    /// <param name="saleId">El identificador único de la venta a eliminar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="Task{bool}"/> que representa la operación asíncrona.
    /// Retorna <c>true</c> si la venta fue eliminada, <c>false</c> si no fue encontrada.</returns>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    Task<bool> DeleteAsync(int saleId, CancellationToken cancellationToken = default);
}
```

## 3. Asesoría Arquitectónica: Impacto de Native AOT en la Implementación de `ISaleRepository`

La elección de Native AOT para el proyecto "Utm_market" tiene implicaciones significativas en la implementación concreta de la interfaz `ISaleRepository`, especialmente en la capa de infraestructura que interactúa con la base de datos. La interfaz en sí misma se mantiene pura y agnóstica a la infraestructura, pero su implementación debe adherirse estrictamente a las restricciones de AOT para cosechar sus beneficios.

### Compatibilidad con Dapper y Native AOT:

La implementación de `ISaleRepository` probablemente utilizará Dapper para el acceso a datos. Es crucial entender que, si bien Dapper es ligero y rápido, su uso tradicional a menudo depende de **reflexión en tiempo de ejecución** para mapear los resultados de la base de datos a objetos C#. Esta reflexión es el principal enemigo del Native AOT, ya que impide el "trimming" agresivo del compilador.

*   **Evitar Mapeo Dinámico:** La implementación NO debe usar `Query<dynamic>()` o cualquier característica de Dapper que mapee automáticamente columnas a propiedades de objetos basándose en nombres de propiedades.
*   **Mapeo Manual o Estático:** Se debe optar por el mapeo explícito de `SqlDataReader` a `VentaEntity` y `DetalleVentaEntity` (o a sus objetos `record` si se usan para DTOs de lectura). Alternativamente, Dapper ofrece formas de mapeo más AOT-friendly como `SqlMapper.SetTypeMap` con `CustomPropertyTypeMap`, o incluso soluciones de generación de código fuente (source generators) para Dapper que están emergiendo y que eliminan la reflexión por completo.
*   **Parámetros de Consulta:** Los parámetros de consulta también deben pasarse explícitamente y no depender de la reflexión para extraer propiedades de un objeto anónimo.

### Riesgo de Consultas N+1 al Cargar Detalles (Loading SaleDetail):

El contrato `ISaleRepository` especifica que cada `Sale` retornada por `GetAllAsync`, `GetByIdAsync` o `FindAsync` debe incluir sus `SaleDetail` cargados. Esto es fundamental para la integridad del agregado. Sin embargo, en la implementación, es un punto crítico para el rendimiento y puede dar lugar al problema de las **consultas N+1**.

*   **¿Qué es N+1?** Sucede cuando, para una colección de N entidades principales, se ejecuta una consulta adicional para cada una de esas N entidades para cargar sus datos relacionados (N + 1 consultas totales). Por ejemplo, si se obtienen 100 ventas, y luego se ejecuta una consulta separada por cada venta para obtener sus detalles, resultan 101 consultas a la base de datos, lo cual es ineficiente.

*   **Soluciones AOT-friendly en Dapper:**
    *   **Multi-mapping (`Query<T1, T2, TResult>`):** Dapper permite mapear múltiples conjuntos de resultados de una sola consulta SQL a objetos relacionados. Por ejemplo, una sola consulta que devuelve tanto los datos de `Venta` como los de `DetalleVenta` puede ser mapeada eficientemente en una llamada a `Query<VentaEntity, DetalleVentaEntity, VentaEntity>` donde Dapper relaciona los detalles a su venta padre. Esto es muy eficiente y AOT-compatible si se hace de forma estática.
    *   **`QueryMultiple`:** Como se mostró en el `SaleMapper` de ejemplo, se pueden ejecutar múltiples consultas SQL en una sola ida y vuelta a la base de datos, y luego leer los resultados de forma secuencial. Esto reduce los viajes de red, aunque sigue siendo necesario unir los datos en el código C#.
    *   **No Cargar Detalles Ansiosamente Siempre:** Para `GetAllAsync` y `FindAsync`, si los detalles no son estrictamente necesarios para cada operación, una estrategia de carga perezosa (`lazy loading`) podría considerarse, aunque esto introduce sus propios retos en un entorno AOT (donde la generación dinámica de proxies es inviable). Idealmente, el contrato de la interfaz debería ser explícito sobre cuándo se cargan los detalles. Dado el diseño actual de la interfaz, se asume la carga "eager" de los detalles.

### Conclusión para la Implementación:

La implementación de `ISaleRepository` requerirá un mapeo de datos cuidadoso y eficiente, preferiblemente utilizando las capacidades de multi-mapping o `QueryMultiple` de Dapper, y evitando cualquier mecanismo que dependa de la reflexión para mantener la compatibilidad con Native AOT y evitar problemas de rendimiento como las consultas N+1. La fase de diseño de la interfaz ha establecido un contrato sólido; ahora la implementación debe cumplir con las exigencias técnicas del runtime de .NET 10.
