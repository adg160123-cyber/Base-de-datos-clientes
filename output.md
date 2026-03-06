## Contrato de Repositorio para el Agregado `Client` (Dominio `Customer`)

### 1. Código Fuente Completo de `IClientRepository.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;
using Utm_market.Core.Filters;

namespace Utm_market.Core.Repositories;

/// <summary>
/// Define el contrato para el repositorio de clientes, aislando el dominio de los detalles de persistencia.
/// Diseñado para ser compatible con Native AOT y optimizado para streaming de datos.
/// </summary>
public interface IClientRepository
{
    /// <summary>
    /// Recupera todos los clientes de forma asíncrona como un flujo de datos.
    /// Esto permite procesar clientes uno por uno sin cargar toda la colección en memoria,
    /// ideal para grandes conjuntos de datos y entornos de consola en .NET 10.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un IAsyncEnumerable de objetos Customer.</returns>
    IAsyncEnumerable<Customer> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera un cliente por su identificador único de forma asíncrona.
    /// </summary>
    /// <param name="id">El identificador único del cliente.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona. El resultado es el cliente encontrado o null si no existe.</returns>
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca clientes que coinciden con los criterios especificados en el filtro.
    /// </summary>
    /// <param name="filter">Objeto CustomerFilter con los criterios de búsqueda.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un IAsyncEnumerable de objetos Customer que coinciden con el filtro.</returns>
    IAsyncEnumerable<Customer> FindAsync(CustomerFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade un nuevo cliente al sistema de forma asíncrona.
    /// </summary>
    /// <param name="customer">El objeto Customer a añadir.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona. El resultado es el cliente añadido, incluyendo cualquier ID generado por la persistencia.</returns>
    Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un cliente existente en el sistema de forma asíncrona.
    /// </summary>
    /// <param name="customer">El objeto Customer con los datos actualizados.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona. El resultado es un booleano indicando si la actualización fue exitosa.</returns>
    Task<bool> UpdateAsync(Customer customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza el estado de actividad de un cliente específico de forma atómica.
    /// </summary>
    /// <param name="id">El identificador único del cliente a actualizar.</param>
    /// <param name="isActive">El nuevo estado de actividad (true para activo, false para inactivo).</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona. El resultado es un booleano indicando si la actualización fue exitosa.</returns>
    Task<bool> UpdateStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un cliente por su identificador único de forma asíncrona.
    /// </summary>
    /// <param name="id">El identificador único del cliente a eliminar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona. El resultado es un booleano indicando si la eliminación fue exitosa.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

### 2. Definición del Objeto `ClientFilter` (Implementado como `CustomerFilter`)

```csharp
namespace Utm_market.Core.Filters;

/// <summary>
/// Representa los criterios de filtro para buscar clientes.
/// </summary>
public record CustomerFilter
{
    /// <summary>
    /// Parte del nombre completo del cliente a buscar (opcional).
    /// </summary>
    public string? FullNameContains { get; init; }

    /// <summary>
    /// Parte del correo electrónico del cliente a buscar (opcional).
    /// </summary>
    public string? EmailContains { get; init; }

    /// <summary>
    /// Estado de actividad del cliente a buscar (opcional).
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Fecha de registro mínima del cliente a buscar (opcional).
    /// </summary>
    public DateTime? MinRegistrationDate { get; init; }

    /// <summary>
    /// Fecha de registro máxima del cliente a buscar (opcional).
    /// </summary>
    public DateTime? MaxRegistrationDate { get; init; }
}
```

### 3. Asesoría Arquitectónica: Impacto de Native AOT en la Implementación del Contrato

La elección de Native AOT para una aplicación .NET 10 (C# 14) tiene implicaciones profundas en la implementación de un contrato de repositorio como `IClientRepository`. El principal objetivo de Native AOT es compilar la aplicación a código máquina nativo antes del despliegue, eliminando la necesidad de compilación JIT (Just-In-Time) en tiempo de ejecución. Esto resulta en:

*   **Menor Huella de Memoria:** Al no incluir el compilador JIT ni metadatos de IL (Intermediate Language) excesivos, los ejecutables AOT son más pequeños y consumen menos memoria.
*   **Arranque Más Rápido:** La aplicación inicia casi instantáneamente porque no hay fase de compilación en tiempo de ejecución.
*   **Rendimiento Predecible:** El rendimiento es más constante ya que no hay pausas para la compilación JIT.

Sin embargo, Native AOT impone restricciones importantes, principalmente relacionadas con el uso de la **reflexión en tiempo de ejecución**. Para garantizar la compatibilidad y un rendimiento óptimo al implementar `IClientRepository` con una base de datos (por ejemplo, usando Dapper), se deben considerar los siguientes puntos:

1.  **Dapper y Generación de Código:**
    *   Dapper, por defecto, utiliza reflexión para mapear resultados de consultas SQL a objetos .NET y para generar SQL a partir de objetos. Si bien Dapper tiene mecanismos de caché, la generación de código en tiempo de ejecución basada en reflexión puede ser problemática con Native AOT.
    *   **Solución:** Se deben utilizar las capacidades de Dapper para la generación de código anticipada (AOT-friendly). Esto a menudo implica:
        *   **Mapeo explícito:** En lugar de depender de la inferencia de Dapper, se pueden usar atributos `[Column]` o mapeadores personalizados (`SqlMapper.SetTypeMap`) para especificar explícitamente cómo las columnas de la base de datos se corresponden con las propiedades de `Customer`.
        *   **Generación de consultas estáticas:** Si es posible, las consultas SQL deben ser cadenas estáticas o generadas de manera predecible para que el analizador AOT pueda "ver" el código de mapeo.
        *   **Contexto de Dapper (ej. `[Dapper.AOT.GenerateSerializer]`)**: Versiones futuras de Dapper o librerías complementarias pueden introducir atributos o herramientas que generen el código de mapeo necesario en tiempo de compilación para evitar la reflexión, similar a cómo funcionan otros ORMs ligeros AOT-friendly.

2.  **`IAsyncEnumerable` y Rendimiento:**
    *   La utilización de `IAsyncEnumerable<Customer>` en el contrato es una excelente práctica para Native AOT. El compilador AOT puede optimizar la lógica de `async`/`await foreach` de manera eficiente, ya que es una característica intrínseca del lenguaje y no depende de reflexión. Esto maximizará los beneficios de streaming en términos de rendimiento y uso de memoria.

3.  **Inmutabilidad y `record`:**
    *   El uso de `record` para `CustomerFilter` y las propiedades `init` en `Customer` (si se usa Primary Constructor para el aggregate root) es ideal para Native AOT. Estas construcciones son estáticas y no involucran reflexión dinámica, lo que permite al compilador AOT optimizar el código de manera más agresiva.

4.  **Nullability (NRTs):**
    *   El uso explícito de `Task<Customer?>` y `string?` con Nullable Reference Types (NRTs) es una buena práctica y totalmente compatible con Native AOT. Mejora la robustez del código sin impacto en el rendimiento AOT.

**Conclusión:**

La implementación del `IClientRepository` para Native AOT requerirá un enfoque cuidadoso, especialmente en la capa de persistencia con Dapper. La clave será minimizar o eliminar la dependencia de la reflexión en tiempo de ejecución en Dapper, utilizando mapeos explícitos y explorando futuras herramientas de generación de código AOT-friendly para el ORM. El contrato en sí mismo, con `IAsyncEnumerable` y NRTs, está bien diseñado para un entorno AOT.
