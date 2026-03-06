# Manifiesto de Componentes y Arquitectura de Referencia para UTN Market CLI

Este documento detalla la inicialización, dependencias y arquitectura de referencia para la aplicación CLI de UTN Market, construida sobre .NET 10 y C# 14, optimizada para Native AOT y principios de Clean Code.

## 1. Resumen de Instalación

La siguiente tabla resume los paquetes NuGet instalados, sus versiones y su rol arquitectónico dentro del proyecto.

| Paquete NuGet                             | Versión | Rol Arquitectónico                                                                |
| :---------------------------------------- | :------ | :-------------------------------------------------------------------------------- |
| `Microsoft.Data.SqlClient`                | 6.1.4   | Driver oficial optimizado para Native AOT para la interacción con bases de datos SQL Server. |
| `Microsoft.Extensions.Hosting`            | 10.0.3  | Fundamental para la inyección de dependencias (DI) y la gestión del ciclo de vida de la aplicación. |
| `Microsoft.Extensions.Configuration.UserSecrets` | 10.0.3  | Proporciona un mecanismo seguro para gestionar secretos de configuración durante el desarrollo local, evitando su exposición en el control de versiones. |

## 2. Referencia de Implementación: `Program.cs` Base

A continuación, se presenta un esqueleto funcional de `Program.cs` que demuestra el uso de C# 14, `HostApplicationBuilder` para la inicialización, y consideraciones para Native AOT.

```csharp
// Program.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data.SqlClient; // Using the Native AOT optimized driver

namespace UtmMarketCli;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configure User Secrets for secure local development
        builder.Configuration.AddUserSecrets<Program>();

        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<IApp, App>();

        var host = builder.Build();

        // Demonstrate a simple service execution
        // In a real CLI, you'd likely have a command dispatcher
        var app = host.Services.GetRequiredService<IApp>();
        await app.RunAsync(CancellationToken.None); // Example with CancellationToken

        await host.RunAsync();
    }
}

// Example Interfaces and Services demonstrating C# 14 features and AOT readiness
public interface IDatabaseService
{
    Task<string> GetProductCountAsync(CancellationToken cancellationToken);
}

public class DatabaseService : IDatabaseService
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionStringField; // C# 14 'field' keyword for properties

    public string ConnectionString
    {
        get => _connectionStringField;
        init => _connectionStringField = value;
    }

    public DatabaseService(IConfiguration configuration)
    {
        _configuration = configuration;
        ConnectionString = _configuration.GetConnectionString("DefaultConnection") 
                           ?? throw new InvalidOperationException("DefaultConnection connection string is not configured.");
    }

    public async ValueTask<string> GetProductCountAsync(CancellationToken cancellationToken) // ValueTask for hot paths
    {
        // AOT Readiness: Avoid reflection. Manual or static mapping is preferred.
        // For example, directly reading values from SqlDataReader.
        // Using Microsoft.Data.SqlClient which is AOT friendly.
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("SELECT COUNT(*) FROM Products", connection);
        var count = (int)(await command.ExecuteScalarAsync(cancellationToken) ?? 0);

        return $"Total products: {count}";
    }
}

public interface IApp
{
    Task RunAsync(CancellationToken cancellationToken);
}

public class App : IApp
{
    private readonly IDatabaseService _databaseService;

    public App(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("UTM Market CLI Application Started.");
        var productCount = await _databaseService.GetProductCountAsync(cancellationToken);
        Console.WriteLine(productCount);
        Console.WriteLine("UTM Market CLI Application Finished.");
    }
}
```

## 3. Notas de Modernización

### Propiedades con la palabra clave `field` (C# 14)
La introducción de la palabra clave `field` en C# 14 permite definir propiedades con un `backing field` explícito de una manera más concisa. Esto reduce el código repetitivo (`boilerplate`) en escenarios donde se necesita un control más fino sobre el campo subyacente de una propiedad, como en `DatabaseService.ConnectionString`. Esto mejora la legibilidad y la mantenibilidad del código.

### Native AOT en .NET 10
.NET 10 con Native AOT (Ahead-of-Time) compilación es fundamental para aplicaciones CLI de alto rendimiento como la que estamos construyendo. Los beneficios clave incluyen:
- **Inicio Más Rápido:** El código se compila a binario nativo en tiempo de publicación, eliminando la necesidad de compilación JIT (Just-In-Time) en tiempo de ejecución.
- **Menor Consumo de Memoria:** Se reduce la superficie de memoria al eliminar componentes del runtime JIT y la necesidad de metadatos adicionales en tiempo de ejecución.
- **Binarios de un Solo Archivo:** Facilita la distribución al generar un único ejecutable sin dependencias del runtime de .NET.
- **Seguridad Mejorada:** La eliminación de la compilación JIT reduce las superficies de ataque potenciales.
- **"Physical Promotion" y Desvirtualización de Interfaces Nativas:** Estas optimizaciones de runtime contribuyen a un código más eficiente y un rendimiento superior, especialmente en rutas de código críticas, al reducir la sobrecarga de llamadas a métodos virtuales y promover objetos a ubicaciones de memoria más óptimas.

La elección de `Microsoft.Data.SqlClient` y la evitación de reflexión en tiempo de ejecución son pasos clave para asegurar la compatibilidad con Native AOT. `ValueTask` se utiliza en `GetProductCountAsync` para optimizar el rendimiento en rutas calientes, evitando la asignación de memoria innecesaria en escenarios asíncronos cuando el resultado ya está disponible sincrónicamente o se puede completar rápidamente.

## 4. Guía de Ejecución

Para compilar y ejecutar esta aplicación como un binario nativo en .NET 10, siga los siguientes pasos:

1.  **Asegúrese de tener el SDK de .NET 10 instalado.**
2.  **Habilite la publicación AOT en el archivo `.csproj`:**
    Agregue las siguientes propiedades al `<PropertyGroup>` en `Utm_market.csproj`:
    ```xml
    <PublishAot>true</PublishAot>
    <InvariantGlobalization>true</InvariantGlobalization>
    <EnableConfigurationBindingGenerator>true</EnableConfigurationBindingGenerator>
    ```
    - `PublishAot`: Habilita la compilación Native AOT.
    - `InvariantGlobalization`: Reduce el tamaño del binario excluyendo datos de globalización que no son necesarios para muchas aplicaciones CLI.
    - `EnableConfigurationBindingGenerator`: Genera código fuente para enlazar la configuración, evitando la reflexión y siendo compatible con AOT.

3.  **Restaurar herramientas y dependencias:**
    ```bash
    dotnet restore
    ```

4.  **Publicar como binario nativo:**
    ```bash
    dotnet publish -r <RID> -c Release /p:PublishAot=true --self-contained
    ```
    Reemplace `<RID>` con el identificador de tiempo de ejecución (Runtime Identifier) de su plataforma de destino, por ejemplo:
    -   `win-x64` para Windows de 64 bits
    -   `linux-x64` para Linux de 64 bits
    -   `osx-x64` para macOS de 64 bits

    Ejemplo para Windows:
    ```bash
    dotnet publish -r win-x64 -c Release /p:PublishAot=true --self-contained
    ```
    Esto generará el ejecutable nativo en la carpeta `bin\Release
et10.0\<RID>\publish`.
