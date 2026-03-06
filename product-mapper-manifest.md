# Diseño e Implementación de Mappers para Entidades (Product)

Este documento detalla el diseño e implementación de la clase `ProductMapper` para el proyecto "Utm_market", que actúa como un puente de datos eficiente y compatible con Native AOT entre la entidad de persistencia (`ProductoEntity`) y el objeto de dominio (`Product`). Se utiliza la sintaxis de 'extension blocks' de C# 14 para una ergonomía y rendimiento óptimos.

## 1. Árbol de Directorios Actualizado

A continuación, se presenta la estructura de directorios del proyecto, mostrando la nueva ubicación de los mappers.

```
C:\code_proyectos\Utm_market
├───db
│   └───scripts
│       ├───01_create_structure_utm_market.sql
│       └───02_seed_data_utm_market.sql
├───obj
│   ├───project.assets.json
│   ├───project.nuget.cache
│   ├───Utm_market.csproj.nuget.dgspec.json
│   ├───Utm_market.csproj.nuget.g.props
│   └───Utm_market.csproj.nuget.g.targets
├───prompts
│   └───database-integration-prompts
│       ├───01-sql-server-architect.md
│       └───02-sql-seeder-mx-products-2025.md
├───src
│   ├───Core
│   │   └───Entities
│   │       ├───Product.cs
│   │       ├───Sale.cs
│   │       ├───SaleDetail.cs
│   │       └───SaleStatus.cs
│   └───Infrastructure
│       ├───Mappers
│       │   └───ProductMapper.cs
│       └───Models
│           └───Data
│               ├───DetalleVentaEntity.cs
│               ├───ProductoEntity.cs
│               └───VentaEntity.cs
├───architectural-manifest.md
├───data-entity-aot-manifest.md
├───entity-design-manifest.md
├───Program.cs
└───Utm_market.csproj
```

## 2. Código Fuente Completo de `ProductMapper.cs`

Esta clase estática contiene métodos de extensión que facilitan la conversión entre los objetos de dominio y de persistencia sin el uso de reflexión, asegurando la compatibilidad con Native AOT.

```csharp
// src/Infrastructure/Mappers/ProductMapper.cs
using UtmMarketCli.Core.Entities;
using UtmMarketCli.Infrastructure.Models.Data;
using System;

namespace UtmMarketCli.Infrastructure.Mappers;

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
            brand: entity.Marca, // Marca is nullable in entity, Product's constructor handles this.
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
```

## 3. Ejemplo de Uso dentro de un Repositorio

El siguiente snippet muestra cómo un repositorio podría utilizar `ProductMapper` para convertir entre objetos de persistencia y de dominio, manteniendo la capa de infraestructura desacoplada de la lógica de negocio directa.

```csharp
// src/Infrastructure/Data/ProductRepository.cs (Hypothetical)
using System.Data.SqlClient;
using Dapper;
using UtmMarketCli.Core.Entities;
using UtmMarketCli.Infrastructure.Models.Data;
using UtmMarketCli.Infrastructure.Mappers; // Importar para usar los métodos de extensión
using Microsoft.Extensions.Configuration;

namespace UtmMarketCli.Infrastructure.Data;

public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("DefaultConnection not configured.");
    }

    /// <summary>
    /// Obtiene un producto por su ID y lo convierte a la entidad de dominio.
    /// </summary>
    /// <param name="productId">El ID del producto.</param>
    /// <returns>El objeto de dominio <see cref="Product"/> o null si no se encuentra.</returns>
    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // Recuperar ProductoEntity (aquí simplificado con Dapper, pero puede ser mapeo manual de SqlDataReader)
        var sql = "SELECT ProductoID, Nombre, SKU, Marca, Precio, Stock FROM Producto WHERE ProductoID = @ProductId";
        var productoEntity = await connection.QueryFirstOrDefaultAsync<ProductoEntity>(sql, new { ProductId = productId });

        // Convertir a la entidad de dominio usando el método de extensión
        return productoEntity?.ToDomain();
    }

    /// <summary>
    /// Añade un nuevo producto a la base de datos convirtiendo la entidad de dominio a la de persistencia.
    /// </summary>
    /// <param name="product">El objeto de dominio <see cref="Product"/> a añadir.</param>
    public async Task AddProductAsync(Product product)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // Convertir la entidad de dominio a la de persistencia usando el método de extensión
        var productoEntity = product.ToEntity();

        var sql = @"INSERT INTO Producto (Nombre, SKU, Marca, Precio, Stock)
                    VALUES (@Nombre, @SKU, @Marca, @Precio, @Stock);"; // SELECT CAST(SCOPE_IDENTITY() as int); if returning ID

        await connection.ExecuteAsync(sql, productoEntity);
    }
}
```

## 4. Justificación del uso de 'extension blocks' de C# 14 en "Utm_market"

El uso de 'extension blocks' (también conocidos como 'extension methods' con sintaxis de C# 14, aunque la terminología oficial se refiere más a `static abstract` miembros en interfaces, aquí se usa en el contexto de métodos de extensión) para `ProductMapper` en el proyecto "Utm_market" ofrece beneficios significativos en rendimiento y legibilidad, especialmente en el contexto de .NET 10 y Native AOT:

1.  **Rendimiento Óptimo (Native AOT-friendly):** Los métodos de extensión son resoluciones en tiempo de compilación. No involucran reflexión ni generación de código dinámico en tiempo de ejecución. Esto es crucial para Native AOT, ya que cualquier dependencia de reflexión puede impedir el `trimming` agresivo del compilador, resultando en binarios más grandes y menor rendimiento. Al ser estáticos y sin reflexión, estos mappers son totalmente compatibles con AOT, garantizando binarios pequeños y un inicio rápido.

2.  **Legibilidad y Ergonomía del Código:** Los métodos de extensión permiten que las llamadas de mapeo se sientan naturales y cohesivas con los objetos que están siendo mapeados. En lugar de escribir `ProductMapper.ToDomain(productoEntity)`, se puede escribir `productoEntity.ToDomain()`. Esto mejora la fluidez del código y hace que las transformaciones de objetos sean más intuitivas de leer y escribir, lo que contribuye a una mejor mantenibilidad.

3.  **Encapsulación y Desacoplamiento:** Al colocar la lógica de mapeo en una clase separada y estática, se mantiene una clara separación de responsabilidades. Las entidades de dominio (`Product`) permanecen puras, libres de cualquier conocimiento sobre la persistencia, y las entidades de persistencia (`ProductoEntity`) no necesitan conocer la lógica de negocio. `ProductMapper` actúa como un conector limpio entre estas capas.

4.  **Menor Overhead:** A diferencia de los inyectables (que requieren servicios de DI y potencialmente asignaciones en el heap), los métodos de extensión estáticos no tienen ningún overhead de instancia. Esto los hace extremadamente ligeros y eficientes para operaciones de mapeo frecuentes, lo que es ideal para aplicaciones de consola de alto rendimiento.

En resumen, los 'extension blocks' en C# 14 proporcionan una forma idiomática, de alto rendimiento y AOT-compatible para manejar las transformaciones de datos en una Clean Architecture, haciendo que el código sea más robusto, eficiente y fácil de mantener en el ecosistema de .NET 10.
