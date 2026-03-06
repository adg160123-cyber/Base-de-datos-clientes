# Clases de Entidad de Base de Datos para Native AOT

Este documento presenta las clases de entidad de base de datos (`ProductoEntity`, `VentaEntity`, `DetalleVentaEntity`) para el proyecto "Utm_market", diseñadas para asegurar la compatibilidad total con Native AOT en .NET 10. Las clases replican el esquema DDL de SQL Server 2022 y utilizan características de C# 14 como la palabra clave `field` y Primary Constructors, manteniendo la pureza de POCOs para un Trimming agresivo.

## 1. Código Fuente Completo para las Tres Entidades

### 1.1. Entidad `ProductoEntity.cs`

Clase parcial que mapea la tabla `Producto` del esquema de base de datos. Incluye validaciones basadas en las restricciones `CHECK` de SQL.

```csharp
// src/Infrastructure/Models/Data/ProductoEntity.cs
using System;

namespace UtmMarketCli.Infrastructure.Models.Data;

public partial class ProductoEntity(
    int productoId,
    string nombre,
    string sku)
{
    public int ProductoID { get; init; } = productoId;
    public string Nombre { get; init; } = nombre;
    public string SKU { get; init; } = sku;

    public string? Marca { get; init; }

    private decimal _precioField;
    public decimal Precio
    {
        get => _precioField;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Precio), "Precio cannot be negative.");
            }
            _precioField = value;
        }
    }

    private int _stockField;
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

### 1.2. Entidad `VentaEntity.cs`

Clase parcial que mapea la tabla `Venta` del esquema de base de datos. Contiene la lógica para las restricciones `CHECK` del `Estatus`.

```csharp
// src/Infrastructure/Models/Data/VentaEntity.cs
using System;

namespace UtmMarketCli.Infrastructure.Models.Data;

public partial class VentaEntity(
    int ventaId,
    string folio)
{
    public int VentaID { get; init; } = ventaId;
    public string Folio { get; init; } = folio;
    public DateTime FechaVenta { get; init; } = DateTime.Now; // DEFAULT GETDATE() in SQL

    private int _totalArticulosField;
    public int TotalArticulos
    {
        get => _totalArticulosField;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(TotalArticulos), "TotalArticulos cannot be negative.");
            }
            _totalArticulosField = value;
        }
    }

    private decimal _totalVentaField;
    public decimal TotalVenta
    {
        get => _totalVentaField;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(TotalVenta), "TotalVenta cannot be negative.");
            }
            _totalVentaField = value;
        }
    }

    private byte _estatusField; // TINYINT in SQL, mapping to byte
    public byte Estatus
    {
        get => _estatusField;
        init
        {
            // CHECK ([Estatus] IN (1, 2, 3))
            if (value != 1 && value != 2 && value != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Estatus), "Estatus must be 1 (Pendiente), 2 (Completada), or 3 (Cancelada).");
            }
            _estatusField = value;
        }
    }
}
```

### 1.3. Entidad `DetalleVentaEntity.cs`

Clase parcial que mapea la tabla `DetalleVenta` del esquema de base de datos.

```csharp
// src/Infrastructure/Models/Data/DetalleVentaEntity.cs
using System;

namespace UtmMarketCli.Infrastructure.Models.Data;

public partial class DetalleVentaEntity(
    int detalleId,
    int ventaId,
    int productoId)
{
    public int DetalleID { get; init; } = detalleId;
    public int VentaID { get; init; } = ventaId;
    public int ProductoID { get; init; } = productoId;

    private decimal _precioUnitarioField;
    public decimal PrecioUnitario
    {
        get => _precioUnitarioField;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(PrecioUnitario), "PrecioUnitario cannot be negative.");
            }
            _precioUnitarioField = value;
        }
    }

    private int _cantidadField;
    public int Cantidad
    {
        get => _cantidadField;
        init
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Cantidad), "Cantidad must be positive.");
            }
            _cantidadField = value;
        }
    }

    private decimal _totalDetalleField;
    public decimal TotalDetalle
    {
        get => _totalDetalleField;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(TotalDetalle), "TotalDetalle cannot be negative.");
            }
            _totalDetalleField = value;
        }
    }
}
```

## 2. Nota Técnica: Mapeo Manual vs. ORMs Dinámicos en .NET 10 y Native AOT

En el contexto de .NET 10 y la compilación Native AOT (Ahead-of-Time), el mapeo manual de datos a través de `SqlDataReader` (o Dapper con mapeo estático/pre-generado) ofrece ventajas significativas de rendimiento y compatibilidad frente a los ORMs dinámicos tradicionales (como Entity Framework Core en su modo de reflexión por defecto).

**Ventajas del Mapeo Manual/Estático en Native AOT:**

1.  **Compatibilidad Total con Trimming:** La compilación Native AOT realiza un "trimming" agresivo del código, eliminando partes del framework y bibliotecas que no se utilizan directamente. Los ORMs que dependen fuertemente de la reflexión en tiempo de ejecución (para descubrir propiedades de clases, construir consultas dinámicamente o instanciar tipos) a menudo confunden el trimmer. Esto puede resultar en la eliminación accidental de código necesario o, lo que es más común, en la necesidad de usar "trimming-disablers" (`DynamicallyAccessedMembers`) que reducen la eficacia del AOT, resultando en binarios más grandes y menos optimizados. El mapeo manual garantiza que solo se incluya el código estrictamente necesario.

2.  **Rendimiento Superior:**
    *   **Menos Sobrecarga de Reflexión:** La reflexión es intrínsecamente más lenta que el acceso directo al código. Al evitar la reflexión, el mapeo manual o pre-generado puede ejecutarse a la velocidad nativa de C#.
    *   **Menor Asignación de Memoria:** Los ORMs dinámicos a menudo asignan memoria para objetos intermedios y metadatos de reflexión. El mapeo manual permite un control más preciso sobre la asignación de memoria, lo que puede ser crítico en escenarios de alto rendimiento y aplicaciones con restricciones de recursos.
    *   **Inicio Más Rápido:** Al no requerir la carga de metadatos de reflexión o la generación dinámica de código en el arranque, las aplicaciones AOT con mapeo manual se inician significativamente más rápido.

3.  **Tamaño de Binario Reducido:** La eliminación de la sobrecarga de reflexión y de las partes del runtime asociadas contribuye directamente a binarios ejecutables más pequeños, lo cual es ideal para distribuciones de aplicaciones CLI y microservicios.

4.  **Control Explicito:** Aunque requiere más código manual, ofrece un control total y explícito sobre cómo se mueven los datos entre la base de datos y los objetos de la aplicación. Esto puede ser una ventaja en sistemas donde el rendimiento es crítico y se necesita una optimización precisa del flujo de datos.

En resumen, mientras que los ORMs dinámicos ofrecen una gran productividad en escenarios de desarrollo rápido y aplicaciones que no son críticas en rendimiento, el mapeo manual o el uso de bibliotecas AOT-friendly como Dapper (con consideraciones para su uso con AOT) son la elección preferida para aplicaciones .NET 10 que buscan maximizar el rendimiento, minimizar el tamaño del binario y asegurar la compatibilidad con la compilación Native AOT.
