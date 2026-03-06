// Program.cs

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Utm_market.Infrastructure;
using Utm_market.Core.UseCases;
using Utm_market.Core.Entities;
using System.Globalization;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace Utm_market;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configuration and Dependency Injection setup
        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Program>();
        }
        builder.Services.AddInfrastructure(builder.Configuration);

        // No need to explicitly register product/sale use cases here if AddInfrastructure handles them or they are not used in the menu
        // builder.Services.AddScoped<IRetrieveAllProductsUseCase, RetrieveAllProductsUseCaseImpl>();
        // builder.Services.AddScoped<IRetrieveProductByIdUseCase, RetrieveProductByIdUseCaseImpl>();
        // builder.Services.AddScoped<IFilterProductsUseCase, FilterProductsUseCaseImpl>();
        // builder.Services.AddScoped<ICreateProductUseCase, CreateProductUseCaseImpl>();
        // builder.Services.AddScoped<IUpdateProductUseCase, UpdateProductUseCaseImpl>();
        // builder.Services.AddScoped<IUpdateProductStockUseCase, UpdateProductStockUseCaseImpl>();

        // builder.Services.AddScoped<IFetchAllSalesUseCase, FetchAllSalesUseCaseImpl>();
        // builder.Services.AddScoped<IFetchSaleByIdUseCase, FetchSaleByIdUseCaseImpl>();
        // builder.Services.AddScoped<IFetchSalesByFilterUseCase, FetchSalesByFilterUseCaseImpl>();
        // builder.Services.AddScoped<ICreateSaleUseCase, CreateSaleUseCaseImpl>();
        // builder.Services.AddScoped<IUpdateSaleStatusUseCase, UpdateSaleStatusUseCaseImpl>();

        var host = builder.Build();

        Console.WriteLine("Infraestructura de Datos Configurada con Éxito.");
        Console.WriteLine("Iniciando aplicación de gestión de clientes...");

        await RunInteractiveMenu(host);
    }

    /// <summary>
    /// Ejecuta el menú interactivo de la aplicación.
    /// </summary>
    /// <param name="host">El host de la aplicación con los servicios configurados.</param>
    private static async Task RunInteractiveMenu(IHost host)
    {
        while (true)
        {
            DisplayMenu();
            var choice = Console.ReadLine();

            // Create a scope for resolving services. Scoped services should not be resolved from the root provider.
            await using var scope = host.Services.CreateAsyncScope();
            var serviceProvider = scope.ServiceProvider;

            switch (choice)
            {
                case "1":
                    await RetrieveAllClients(serviceProvider);
                    break;
                case "2":
                    await RetrieveClientById(serviceProvider);
                    break;
                case "3":
                    await RegisterNewClient(serviceProvider);
                    break;
                case "4":
                    Console.WriteLine("Saliendo de la aplicación. ¡Hasta pronto!");
                    return;
                default:
                    Console.WriteLine("Opción no válida. Por favor, intente de nuevo.");
                    break;
            }
            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Muestra las opciones del menú principal.
    /// </summary>
    private static void DisplayMenu()
    {
        Console.Clear();
        Console.WriteLine("""
            =========================================
                 Menú de Gestión de Clientes
            =========================================
            1. Consultar todos los clientes
            2. Consultar cliente por ID
            3. Registrar nuevo cliente
            4. Salir
            =========================================
            Seleccione una opción:
            """);
    }

    /// <summary>
    /// Recupera y muestra todos los clientes.
    /// </summary>
    /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
    private static async Task RetrieveAllClients(IServiceProvider serviceProvider)
    {
        Console.WriteLine("\n--- Consultar Todos los Clientes ---");
        var useCase = serviceProvider.GetRequiredService<IRetrieveAllClientsUseCase>();
        var clients = useCase.ExecuteAsync();

        var clientCount = 0;
        await foreach (var client in clients)
        {
            Console.WriteLine($"ID: {client.ClienteID}, Nombre: {client.Nombre} {client.ApellidoPaterno}, Email: {client.Email}, Activo: {client.Activo}");
            clientCount++;
        }

        if (clientCount == 0)
        {
            Console.WriteLine("No se encontraron clientes.");
        }
    }

    /// <summary>
    /// Recupera y muestra un cliente por su ID.
    /// </summary>
    /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
    private static async Task RetrieveClientById(IServiceProvider serviceProvider)
    {
        Console.WriteLine("\n--- Consultar Cliente por ID ---");
        Console.Write("Ingrese el ID del cliente: ");
        if (!int.TryParse(Console.ReadLine(), out int clientId))
        {
            Console.WriteLine("ID inválido. Por favor, ingrese un número entero.");
            return;
        }

        var useCase = serviceProvider.GetRequiredService<IRetrieveClientByIdUseCase>();
        var client = await useCase.ExecuteAsync(clientId);

        if (client != null)
        {
            Console.WriteLine($"\nCliente Encontrado:");
            Console.WriteLine($"ID: {client.ClienteID}");
            Console.WriteLine($"Nombre: {client.Nombre} {client.ApellidoPaterno} {client.ApellidoMaterno ?? ""}".Trim());
            Console.WriteLine($"Email: {client.Email}");
            Console.WriteLine($"Teléfono: {client.Telefono ?? "N/A"}");
            Console.WriteLine($"Dirección: {client.Direccion ?? "N/A"}");
            Console.WriteLine($"Fecha Registro: {client.FechaRegistro:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Activo: {(client.Activo ? "Sí" : "No")}");
        }
        else
        {
            Console.WriteLine($"No se encontró ningún cliente con ID: {clientId}");
        }
    }

    /// <summary>
    /// Registra un nuevo cliente solicitando los datos al usuario.
    /// </summary>
    /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
    private static async Task RegisterNewClient(IServiceProvider serviceProvider)
    {
        Console.WriteLine("\n--- Registrar Nuevo Cliente ---");

        string? nombre;
        do
        {
            Console.Write("Nombre (obligatorio): ");
            nombre = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nombre)) Console.WriteLine("El nombre no puede estar vacío.");
        } while (string.IsNullOrWhiteSpace(nombre));

        string? apellidoPaterno;
        do
        {
            Console.Write("Apellido Paterno (obligatorio): ");
            apellidoPaterno = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(apellidoPaterno)) Console.WriteLine("El apellido paterno no puede estar vacío.");
        } while (string.IsNullOrWhiteSpace(apellidoPaterno));

        Console.Write("Apellido Materno (opcional): ");
        string? apellidoMaterno = Console.ReadLine();

        string? email;
        do
        {
            Console.Write("Email (obligatorio y válido): ");
            email = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(email)) Console.WriteLine("El email no puede estar vacío.");
            else if (!IsValidEmail(email)) Console.WriteLine("Formato de email inválido.");
        } while (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email));

        Console.Write("Teléfono (opcional): ");
        string? telefono = Console.ReadLine();

        Console.Write("Dirección (opcional): ");
        string? direccion = Console.ReadLine();

        var newClient = new Client(
            nombre!,
            apellidoPaterno!,
            email!,
            apellidoMaterno,
            telefono,
            direccion
        );

        try
        {
            var createClientUseCase = serviceProvider.GetRequiredService<ICreateClientUseCase>();
            var createdClient = await createClientUseCase.ExecuteAsync(newClient);
            Console.WriteLine($"\nCliente '{createdClient.Nombre} {createdClient.ApellidoPaterno}' registrado con éxito. ID: {createdClient.ClienteID}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error de validación al registrar cliente: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            // Catch for potential database-level issues like unique constraint violations
            Console.WriteLine($"Error al registrar cliente: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida el formato básico de un correo electrónico.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        // This regex is a simple check; a more robust solution might use System.Net.Mail.MailAddress,
        // but that might have AOT trimming issues. For CLI validation, this is often sufficient.
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
