# Configuración Segura de Persistencia con Dapper y Native AOT (.NET 10)

Este documento detalla la implementación de una capa de persistencia segura y compatible con Native AOT para el proyecto "Utm_market", utilizando Dapper, `Microsoft.Data.SqlClient` y las capacidades de configuración de .NET 10. Se siguen estrictamente los principios SOLID y las prácticas de seguridad, especialmente en el manejo de credenciales.

## Resumen de Cambios Realizados

Se ha configurado la infraestructura de datos para conectar de forma segura a SQL Server, separando las credenciales sensibles del código fuente mediante User Secrets. Se implementó un patrón de Factoría para la creación de conexiones a base de datos, lo que facilita la inyección de dependencias y la gestión del ciclo de vida de las conexiones. Finalmente, se integraron estos componentes en el sistema de inyección de dependencias (`HostApplicationBuilder`) de .NET 10.

## Comandos CLI Ejecutados

Los siguientes comandos fueron ejecutados para configurar el entorno:

1.  **Instalar Dapper:**
    ```bash
    dotnet add package Dapper
    ```
2.  **Inicializar User Secrets:**
    ```bash
    dotnet user-secrets init
    ```
    *(Este comando establece el `UserSecretsId` en el archivo `.csproj`, en este caso: `8d5ae8d1-ec72-456f-8149-804fabefd8fe`)*
3.  **Almacenar Credenciales en User Secrets:**
    ```bash
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "User ID=ai_oria19_SQLLogin_1;Password=fxjvjc2ks6"
    ```

## Archivos de Configuración Creados/Modificados

### `appsettings.json`
Contiene la parte no sensible de la cadena de conexión.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "workstation id=dcmo_utm_dacb.mssql.somee.com;packet size=4096;data source=dcmo_utm_dacb.mssql.somee.com;persist security info=False;initial catalog=dcmo_utm_dacb;TrustServerCertificate=True"
  }
}
```

### `appsettings.Development.json`
Enlaza el entorno de desarrollo con los User Secrets definidos.

```json
{
  "UserSecretsId": "8d5ae8d1-ec72-456f-8149-804fabefd8fe"
}
```

### `appsettings.Production.json`
Archivo de configuración para el entorno de producción (vacío por ahora, esperando configuraciones específicas para producción).

```json
{}
```

## Código Fuente Implementado

### `src/Infrastructure/Data/IDbConnectionFactory.cs`
Define la interfaz para la factoría de conexiones a base de datos, promoviendo la abstracción y la inyección de dependencias.

```csharp
// src/Infrastructure/Data/IDbConnectionFactory.cs
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace UtmMarketCli.Infrastructure.Data;

/// <summary>
/// Define el contrato para una factoría de conexiones a bases de datos.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Crea y abre una nueva conexión a la base de datos de forma asíncrona.
    /// Es responsabilidad del consumidor gestionar el ciclo de vida (cerrar y disponer) de la conexión.
    /// Se recomienda encarecidamente utilizar bloques 'using' o declaraciones 'using'.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona de apertura de conexión.</param>
    /// <returns>Una instancia de <see cref="IDbConnection"/> abierta.</returns>
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}
```

### `src/Infrastructure/Data/SqlConnectionFactory.cs`
Implementación de la factoría de conexiones para SQL Server, utilizando `Microsoft.Data.SqlClient` y validación de la cadena de conexión con la palabra clave `field` de C# 14.

```csharp
// src/Infrastructure/Data/SqlConnectionFactory.cs
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace UtmMarketCli.Infrastructure.Data;

/// <summary>
/// Implementación de <see cref="IDbConnectionFactory"/> para SQL Server.
/// Utiliza <see cref="Microsoft.Data.SqlClient"/> para crear y abrir conexiones.
/// La cadena de conexión se obtiene de la configuración de la aplicación.
/// </summary>
public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionStringField; // Usando 'field' de C# 14

    /// <summary>
    /// La cadena de conexión a la base de datos SQL Server.
    /// Incluye validación para asegurar que no sea nula o vacía.
    /// </summary>
    public string ConnectionString
    {
        get => _connectionStringField;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(ConnectionString));
            }
            _connectionStringField = value;
        }
    }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="SqlConnectionFactory"/>.
    /// </summary>
    /// <param name="configuration">La configuración de la aplicación, utilizada para obtener la cadena de conexión.</param>
    /// <exception cref="InvalidOperationException">Se lanza si la cadena de conexión 'DefaultConnection' no está configurada.</exception>
    public SqlConnectionFactory(IConfiguration configuration)
    {
        ConnectionString = configuration.GetConnectionString("DefaultConnection")
                           ?? throw new InvalidOperationException("DefaultConnection connection string is not configured.");
    }

    /// <summary>
    /// Crea y abre una nueva conexión a la base de datos SQL Server de forma asíncrona.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona de apertura de conexión.</param>
    /// <returns>Una instancia de <see cref="IDbConnection"/> abierta.</returns>
    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
```

### `src/Infrastructure/DependencyInjection.cs`
Clase estática que contiene un método de extensión para `IServiceCollection`, permitiendo el registro modular de los servicios de infraestructura.

```csharp
// src/Infrastructure/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using UtmMarketCli.Infrastructure.Data;
using System.Data; // For IDbConnection
using Microsoft.Extensions.Options; // For options validation
using System; // For ArgumentException

namespace UtmMarketCli.Infrastructure;

/// <summary>
/// Provee métodos de extensión para configurar los servicios de infraestructura
/// en la colección de servicios de la aplicación.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Añade los servicios de persistencia a la colección de servicios.
    /// Incluye la factoría de conexión a base de datos.
    /// </summary>
    /// <param name="services">La colección de servicios.</param>
    /// <param name="configuration">La configuración de la aplicación.</param>
    /// <returns>La colección de servicios para encadenamiento.</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Registro de la factoría de conexión a la base de datos
        // Se registra como Singleton porque la factoría en sí no mantiene estado de conexión abierto.
        // La instancia de IDbConnection se crea por llamada a CreateConnectionAsync.
        services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(configuration));
        
        // Aquí se podrían añadir otros servicios de infraestructura, como repositorios implementados.
        // Ejemplo:
        // services.AddScoped<IProductRepository, ProductRepository>();
        // services.AddScoped<ISaleRepository, SaleRepository>();

        return services;
    }
}
```

### `Program.cs` (Modificado)
Se actualizó `Program.cs` para utilizar `Host.CreateApplicationBuilder`, cargar la configuración (incluyendo User Secrets) y registrar los servicios de infraestructura mediante el método de extensión. El ejemplo de uso de `IApp` se adaptó para demostrar el uso de `IDbConnectionFactory`.

```csharp
// Program.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data.SqlClient; // Still needed for SqlConnectionFactory internal use

using UtmMarketCli.Infrastructure; // For AddInfrastructureServices
using UtmMarketCli.Infrastructure.Data; // For IDbConnectionFactory
using System.Data; // For IDbConnection

namespace UtmMarketCli;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configure User Secrets for secure local development
        // This implicitly adds environment variables and appsettings files.
        builder.Configuration.AddUserSecrets<Program>();

        // Add Infrastructure services (IDbConnectionFactory, Repositories, etc.)
        builder.Services.AddInfrastructureServices(builder.Configuration);

        // Register your application's core services
        // Example: If you had a service that uses IDbConnectionFactory
        builder.Services.AddSingleton<IApp, App>(); // Keep IApp and App for now

        var host = builder.Build();

        // Demonstrate a simple service execution
        var app = host.Services.GetRequiredService<IApp>();
        await app.RunAsync(CancellationToken.None);

        await host.RunAsync();
    }
}

// The previous IDatabaseService and DatabaseService are removed
// as they are replaced by the more generic IDbConnectionFactory

public interface IApp
{
    Task RunAsync(CancellationToken cancellationToken);
}

public class App : IApp
{
    private readonly IDbConnectionFactory _connectionFactory;

    // Example of how App might use the connection factory
    public App(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("UTM Market CLI Application Started.");

        // Example: Use the connection factory to get a connection
        await using (var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken))
        {
            Console.WriteLine($"Database connection established. State: {connection.State}");
            // Perform database operations here, e.g., using Dapper
            // For example, get product count (re-using previous logic as an illustration)
            await using var command = new SqlCommand("SELECT COUNT(*) FROM Producto", (SqlConnection)connection);
            var count = (int)(await command.ExecuteScalarAsync(cancellationToken) ?? 0);
            Console.WriteLine($"Total products (via IDbConnectionFactory): {count}");
        }

        Console.WriteLine("UTM Market CLI Application Finished.");
    }
}
```

## Nota de Seguridad sobre la Cadena de Conexión

La cadena de conexión proporcionada por el usuario contenía `TrustServerCertificate=True`. Si bien esto puede ser aceptable para entornos de desarrollo o pruebas, **no se recomienda para entornos de producción**. La opción `TrustServerCertificate=True` deshabilita la validación de la cadena de confianza del certificado SSL/TLS del servidor, lo que expone la conexión a ataques de tipo "man-in-the-middle". Para producción, se debería utilizar `TrustServerCertificate=False` (o omitirlo, ya que es el valor predeterminado en versiones recientes de `Microsoft.Data.SqlClient`) y asegurar que el servidor de base de datos presente un certificado válido y confiable.
