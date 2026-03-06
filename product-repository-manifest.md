# Diseño del Contrato de Repositorio para la Entidad Product

Este documento define el contrato (`IProductRepository`) para la gestión de la entidad de dominio `Product` dentro del proyecto "Utm_market". Siguiendo los principios de Clean Architecture, este contrato se ubica en la capa Core y asegura una abstracción pura de la persistencia, trabajando exclusivamente con objetos de dominio y garantizando la compatibilidad con Native AOT en .NET 10.

## 1. Estructura de Namespaces Recomendada

La siguiente estructura muestra la organización de los namespaces dentro de la capa `src/Core` del proyecto:

```
src\Core
├───Entities\        (Contiene las entidades de dominio puras: Product, Sale, SaleDetail, SaleStatus)
├───Filters\         (Contiene objetos de criterios para búsquedas, como ProductFilter)
└───Repositories\    (Contiene las interfaces de los repositorios, como IProductRepository)
```

## 2. Código Fuente Completo de `ProductFilter.cs`

`ProductFilter` es una clase record que define los criterios de búsqueda para la entidad `Product`.

```csharp
// src/Core/Filters/ProductFilter.cs
using System;

namespace UtmMarketCli.Core.Filters;

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
```

## 3. Código Fuente Completo de `IProductRepository.cs`

Esta interfaz define las operaciones CRUD y de consulta para la entidad `Product`, asegurando la inyección de dependencias y la compatibilidad con operaciones asíncronas y eficientes en memoria.

```csharp
// src/Core/Repositories/IProductRepository.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UtmMarketCli.Core.Entities;
using UtmMarketCli.Core.Filters; // Para ProductFilter

namespace UtmMarketCli.Core.Repositories;

/// <summary>
/// Define el contrato para el repositorio de productos.
/// Esta interfaz es parte de la capa de Core (Dominio) y solo maneja objetos de dominio (<see cref="Product"/>).
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Recupera todos los productos de forma asíncrona como un flujo de datos.
    /// Utiliza <see cref="IAsyncEnumerable{T}"/> para un manejo eficiente de la memoria
    /// al permitir el streaming de resultados.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="IAsyncEnumerable{Product}"/> que representa un flujo de productos.</returns>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    IAsyncEnumerable<Product> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera un producto por su identificador único de forma asíncrona.
    /// </summary>
    /// <param name="productId">El identificador único del producto.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="Task{Product}"/> que contendrá el producto encontrado, o null si no existe.</returns>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    Task<Product?> GetByIdAsync(int productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca productos que coincidan con los criterios especificados en el filtro.
    /// </summary>
    /// <param name="filter">Objeto <see cref="ProductFilter"/> con los criterios de búsqueda.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="IAsyncEnumerable{Product}"/> que representa un flujo de productos que coinciden con el filtro.</returns>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    IAsyncEnumerable<Product> FindAsync(ProductFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade un nuevo producto al repositorio de forma asíncrona.
    /// </summary>
    /// <param name="product">El objeto <see cref="Product"/> a añadir. Su <see cref="Product.ProductID"/> será actualizado
    /// si la base de datos genera un nuevo ID.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="Task"/> que representa la operación asíncrona.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si el producto es nulo.</exception>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un producto existente en el repositorio de forma asíncrona.
    /// </summary>
    /// <param name="product">El objeto <see cref="Product"/> con los datos actualizados.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="Task"/> que representa la operación asíncrona.
    /// Retorna <c>true</c> si el producto fue encontrado y actualizado, <c>false</c> en caso contrario.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si el producto es nulo.</exception>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza de forma atómica el stock de un producto específico.
    /// </summary>
    /// <param name="productId">El identificador único del producto.</param>
    /// <param name="newStock">La nueva cantidad de stock para el producto. Debe ser no negativa.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="Task{bool}"/> que representa la operación asíncrona.
    /// Retorna <c>true</c> si el stock fue actualizado, <c>false</c> si el producto no fue encontrado.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Se lanza si <paramref name="newStock"/> es negativo.</exception>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    Task<bool> UpdateStockAsync(int productId, int newStock, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un producto del repositorio de forma asíncrona.
    /// </summary>
    /// <param name="productId">El identificador único del producto a eliminar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="Task{bool}"/> que representa la operación asíncrona.
    /// Retorna <c>true</c> si el producto fue eliminado, <c>false</c> si no fue encontrado.</returns>
    /// <exception cref="OperationCanceledException">Se lanza si la operación es cancelada.</exception>
    Task<bool> DeleteAsync(int productId, CancellationToken cancellationToken = default);
}
```

## 4. Explicación Técnica: `IAsyncEnumerable` para Aplicaciones de Consola en .NET 10

En el contexto de aplicaciones de consola en .NET 10, especialmente aquellas optimizadas para Native AOT y bajo consumo de memoria, el uso de `IAsyncEnumerable<T>` para recuperar colecciones de datos ofrece ventajas significativas sobre los enfoques tradicionales que retornan `Task<List<T>>` o `Task<IEnumerable<T>>`.

### ¿Qué es `IAsyncEnumerable<T>`?

Introducido en C# 8, `IAsyncEnumerable<T>` permite la iteración asíncrona de colecciones. A diferencia de un `IEnumerable<T>` que opera sincrónicamente, o un `Task<List<T>>` que carga toda la colección en memoria antes de retornarla, `IAsyncEnumerable<T>` permite que los elementos de una colección se produzcan y se consuman de forma asíncrona, uno a uno, a medida que están disponibles. Esto se logra mediante la palabra clave `yield return` dentro de un método `async`.

### Beneficios para Aplicaciones de Consola en .NET 10:

1.  **Optimización de Memoria (Streaming de Datos):**
    *   **Consumo Progresivo:** La ventaja más importante para una CLI es que `IAsyncEnumerable` permite procesar elementos a medida que se obtienen de la fuente de datos (por ejemplo, una base de datos). Esto evita cargar toda una colección potencialmente grande en memoria de una sola vez, lo que es crítico para aplicaciones con restricciones de memoria o al trabajar con grandes volúmenes de datos. Para una aplicación de consola que podría ejecutar scripts o procesar informes, esto significa que no colapsará por `OutOfMemoryException` si el dataset es enorme.
    *   **Bajo Overhead:** El uso de `yield return` minimiza la asignación de memoria necesaria en comparación con construir una `List<T>` completa antes de retornar.

2.  **Rendimiento Mejorado y Responsividad:**
    *   **Procesamiento Paralelo (implícito):** Al no tener que esperar a que toda la operación de recuperación de datos finalice, la aplicación puede comenzar a procesar los primeros elementos mientras los elementos restantes aún se están obteniendo de forma asíncrona. Esto puede mejorar la percepción de responsividad y, en algunos casos, el rendimiento total.
    *   **Latencia Reducida:** Si un usuario (o un script) solo necesita los primeros `N` resultados de una consulta grande, `IAsyncEnumerable` permite obtener esos `N` resultados y continuar con el procesamiento sin tener que esperar a que se obtengan los `M` resultados restantes.

3.  **Compatibilidad Natural con .NET 10 y Native AOT:**
    *   **Idiomático:** `IAsyncEnumerable` es una característica moderna de C# diseñada para funcionar bien con el modelo asíncrono de .NET, lo que lo hace idiomático para el desarrollo en .NET 10.
    *   **AOT-Friendly:** Al ser una característica del lenguaje y del framework base, no depende de reflexión o técnicas dinámicas, lo que lo hace completamente compatible con Native AOT y el `trimming` agresivo que busca el máximo rendimiento y un tamaño de binario reducido.

4.  **Flexibilidad en el Consumo:**
    *   Se puede consumir utilizando un bucle `await foreach`, que es una construcción familiar y fácil de usar en C#.
    *   Permite a la capa de aplicación decidir cuándo y cuántos elementos consumir, lo que es útil para paginación o para aplicar lógica de "short-circuiting" en el procesamiento.

En resumen, `IAsyncEnumerable<T>` es una elección arquitectónica superior para métodos de repositorio que devuelven colecciones en .NET 10, especialmente en aplicaciones de consola donde el control del consumo de memoria y la capacidad de streaming de datos son fundamentales para la eficiencia y la escalabilidad.
