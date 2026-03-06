// src/Infrastructure/DependencyInjection.cs

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Utm_market.Infrastructure.Data;
using Utm_market.Core.Repositories;
using Utm_market.Infrastructure.Repositories;
using Utm_market.Core.UseCases; // Add this using directive
using Utm_market.Application; // Add this using directive
using System;

namespace Utm_market.Infrastructure;

/// <summary>
/// Proporciona métodos de extensión para la configuración de la capa de infraestructura
/// en la colección de servicios de .NET Core, facilitando la inyección de dependencias.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Agrega y configura los servicios de la capa de infraestructura a la colección de servicios.
    /// </summary>
    /// <param name="services">La colección de servicios a la que se agregarán las dependencias.</param>
    /// <param name="configuration">La configuración de la aplicación.</param>
    /// <returns>La colección de servicios con las dependencias de infraestructura agregadas.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Registro de la fábrica de conexiones a la base de datos
        services.AddSingleton<IDbConnectionFactory>(provider =>
        {
            return new SqlConnectionFactory(configuration);
        });

        // Registro de repositorios
        services.AddScoped<IClientRepository, ClientRepositoryImpl>();

        // Registro de casos de uso de Clientes
        services.AddScoped<IRetrieveAllClientsUseCase, RetrieveAllClientsUseCaseImpl>();
        services.AddScoped<IRetrieveClientByIdUseCase, RetrieveClientByIdUseCaseImpl>();
        services.AddScoped<ICreateClientUseCase, CreateClientUseCaseImpl>();

        // Opcional: Para el patrón ValidateOnStart, si fuera necesario para otras opciones
        // services.AddOptions<DbConnectionOptions>()
        //     .Bind(configuration.GetSection("ConnectionStrings"))
        //     .ValidateDataAnnotations()
        //     .ValidateOnStart();

        return services;
    }
}

