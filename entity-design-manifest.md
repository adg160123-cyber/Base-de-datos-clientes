# Diseño de Entidades de Dominio Puras para UTN Market

Este documento presenta el diseño de las entidades de dominio `Product`, `SaleDetail` y `Sale` para el proyecto "Utm_market", siguiendo los principios de Domain-Driven Design (DDD), Clean Architecture, y optimizaciones para Native AOT en .NET 10 y C# 14. Se ha priorizado la pureza del dominio, encapsulando la lógica de negocio y evitando dependencias de ORM o atributos de persistencia en esta capa.

## 1. Definición del Enum de Estatus de Venta

El enumerador `SaleStatus` define los posibles estados de una venta dentro del sistema.

```csharp
// src/Core/Entities/SaleStatus.cs
namespace UtmMarketCli.Core.Entities;

public enum SaleStatus
{
    /// <summary>
    /// The sale has been initiated but not yet completed.
    /// </summary>
    Pending,

    /// <summary>
    /// The sale has been successfully completed.
    /// </summary>
    Completed,

    /// <summary>
    /// The sale has been cancelled.
    /// </summary>
    Cancelled,

    /// <summary>
    /// The sale is awaiting payment.
    /// </summary>
    AwaitingPayment
}
```

## 2. Código Fuente Completo y Documentado para Cada Clase

### 2.1. Entidad `Product.cs`

Representa un producto dentro del sistema, incluyendo validaciones de negocio en sus propiedades.

```csharp
// src/Core/Entities/Product.cs
namespace UtmMarketCli.Core.Entities;

public class Product(
    int productId,
    string name,
    string sku,
    string brand,
    decimal price,
    int stock)
{
    public int ProductID { get; init; } = productId;
    public string Name { get; init; } = name;
    public string SKU { get; init; } = sku;
    public string Brand { get; init; } = brand;

    private decimal _priceField; // C# 14 'field' keyword for backing field
    public decimal Price
    {
        get => _priceField;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Price), "Price cannot be negative.");
            }
            _priceField = value;
        }
    }

    private int _stockField; // C# 14 'field' keyword for backing field
    public int Stock
    {
        get => _stockField;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Stock), "Stock cannot be negative.");
            }
            _stockField = value;
        }
    }
}
```

### 2.2. Entidad `SaleDetail.cs`

Representa un ítem dentro de una venta, capturando el precio unitario en el momento de la venta y calculando el total por detalle.

```csharp
// src/Core/Entities/SaleDetail.cs
namespace UtmMarketCli.Core.Entities;

public class SaleDetail(
    int saleDetailId,
    int productId,
    int quantity,
    decimal unitPrice)
{
    public int SaleDetailID { get; init; } = saleDetailId;
    public int ProductID { get; init; } = productId; // Reference to Product ID

    private int _quantityField; // C# 14 'field' keyword for backing field
    public int Quantity
    {
        get => _quantityField;
        init
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Quantity), "Quantity must be positive.");
            }
            _quantityField = value;
        }
    }

    public decimal UnitPrice { get; init; } = unitPrice; // Price at the time of sale, independent of current product price

    /// <summary>
    /// Calculates the total for this sale detail (UnitPrice * Quantity).
    /// </summary>
    public decimal TotalDetail => UnitPrice * Quantity;
}
```

### 2.3. Entidad `Sale.cs`

Representa una venta completa, incluyendo su fecha, estado, una colección de detalles de venta y propiedades agregadas calculadas.

```csharp
// src/Core/Entities/Sale.cs
using System.Collections.Generic;
using System.Linq;

namespace UtmMarketCli.Core.Entities;

public class Sale(
    int saleId,
    string folio,
    SaleStatus status)
{
    public int SaleID { get; init; } = saleId;
    public string Folio { get; init; } = folio;
    public DateTime SaleDate { get; init; } = DateTime.Now; // Automatically initialized
    public SaleStatus Status { get; init; } = status;

    private readonly List<SaleDetail> _saleDetailsField = new(); // C# 14 'field' keyword for backing field
    public IReadOnlyCollection<SaleDetail> SaleDetails
    {
        get => _saleDetailsField.AsReadOnly();
        init => _saleDetailsField = new List<SaleDetail>(value); // Allows initialization with a collection
    }

    /// <summary>
    /// Adds a new SaleDetail to the sale.
    /// </summary>
    /// <param name="detail">The SaleDetail to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if the detail is null.</exception>
    public void AddSaleDetail(SaleDetail detail)
    {
        if (detail == null)
        {
            throw new ArgumentNullException(nameof(detail));
        }
        _saleDetailsField.Add(detail);
    }

    /// <summary>
    /// Calculates the total number of items across all sale details.
    /// </summary>
    public int TotalItems => SaleDetails.Sum(detail => detail.Quantity);

    /// <summary>
    /// Calculates the total sale amount across all sale details.
    /// </summary>
    public decimal TotalSale => SaleDetails.Sum(detail => detail.TotalDetail);
}
```

## 3. Explicación de la palabra clave `field` y reducción de boilerplate

La palabra clave `field` en C# 14 es una adición poderosa que permite una sintaxis más concisa y legible para definir propiedades con un "backing field" explícito. Antes de C# 14, para implementar lógica personalizada en los accesores `get` o `set` de una propiedad (como validaciones), era necesario declarar manualmente un campo privado que almacenara el valor de la propiedad, lo que aumentaba el `boilerplate` (código repetitivo).

**Cómo `field` reduce el boilerplate en las validaciones de negocio:**

En las entidades `Product` (`Price`, `Stock`) y `SaleDetail` (`Quantity`), hemos utilizado `field` para las validaciones en el accesor `init`.

**Ejemplo sin `field` (pre-C# 14):**

```csharp
public class ProductOld
{
    private decimal _price; // Manual backing field
    public decimal Price
    {
        get => _price;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Price), "Price cannot be negative.");
            }
            _price = value; // Asignación al backing field
        }
    }
}
```

**Ejemplo con `field` (C# 14):**

```csharp
public class ProductNew
{
    public decimal Price
    {
        get => field; // 'field' keyword references the implicit backing field
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Price), "Price cannot be negative.");
            }
            field = value; // Asignación al implicit backing field
        }
    }
}
```

Como se puede observar, el uso de `field` elimina la necesidad de declarar explícitamente `_priceField` (y sus variantes) y gestionarlo manualmente. El compilador se encarga de crear el campo de respaldo implícitamente, lo que resulta en un código más limpio, menos propenso a errores y más directo para expresar la lógica de validación o cualquier otro comportamiento personalizado en los accesores. Esto es especialmente beneficioso en la capa de dominio, donde las validaciones de las reglas de negocio son cruciales y deben estar lo más cerca posible de la definición de la propiedad a la que se aplican.
