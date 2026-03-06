// src/Core/Entities/Client.cs

using System;

namespace Utm_market.Core.Entities;

/// <summary>
/// Representa la entidad de dominio Cliente, siguiendo los principios de Domain-Driven Design (DDD).
/// Esta clase es un POCO puro, diseñado para ser compatible con Native AOT en .NET 10.
/// </summary>
public sealed class Client
{

    /// <summary>
    /// Constructor primario para inicializar un nuevo cliente.
    /// ClienteID se inicializa a 0 y se espera que la base de datos lo asigne.
    /// FechaRegistro se inicializa automáticamente.
    /// Activo por defecto es true.
    /// </summary>
    /// <param name="nombre">Nombre del cliente.</param>
    /// <param name="apellidoPaterno">Apellido paterno del cliente.</param>
    /// <param name="email">Correo electrónico único del cliente.</param>
    /// <param name="apellidoMaterno">Apellido materno del cliente (opcional).</param>
    /// <param name="telefono">Número de teléfono del cliente (opcional).</param>
    /// <param name="direccion">Dirección del cliente (opcional).</param>
    public Client(string nombre, string apellidoPaterno, string email, string? apellidoMaterno = null, string? telefono = null, string? direccion = null)
    {
        Nombre = nombre;
        ApellidoPaterno = apellidoPaterno;
        Email = email;
        ApellidoMaterno = apellidoMaterno;
        Telefono = telefono;
        Direccion = direccion;
        FechaRegistro = DateTime.Now; // Se inicializa al momento de la creación de la entidad
        Activo = true; // Por defecto un cliente nuevo está activo
    }

    /// <summary>
    /// Constructor para clientes existentes (usado por mappers o deserialización).
    /// </summary>
    public Client(int clienteId, string nombre, string apellidoPaterno, string email, DateTime fechaRegistro, bool activo, string? apellidoMaterno = null, string? telefono = null, string? direccion = null)
    {
        if (clienteId <= 0)
            throw new ArgumentException("ClienteID debe ser mayor que cero para un cliente existente.", nameof(clienteId));

        ClienteID = clienteId;
        Nombre = nombre;
        ApellidoPaterno = apellidoPaterno;
        Email = email;
        FechaRegistro = fechaRegistro;
        Activo = activo;
        ApellidoMaterno = apellidoMaterno;
        Telefono = telefono;
        Direccion = direccion;
    }

    /// <summary>
    /// Identificador único del cliente.
    /// </summary>
    public int ClienteID
    {
        get => field;
        private set
        {
            if (value <= 0)
                throw new ArgumentException("ClienteID no puede ser menor o igual a cero.", nameof(ClienteID));
            field = value;
        }
    }

    /// <summary>
    /// Nombre del cliente. No puede ser nulo ni estar vacío.
    /// </summary>
    public string Nombre
    {
        get => field;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El nombre del cliente no puede estar vacío.", nameof(Nombre));
            field = value;
        }
    }

    /// <summary>
    /// Apellido paterno del cliente. No puede ser nulo ni estar vacío.
    /// </summary>
    public string ApellidoPaterno
    {
        get => field;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El apellido paterno del cliente no puede estar vacío.", nameof(ApellidoPaterno));
            field = value;
        }
    }

    /// <summary>
    /// Apellido materno del cliente. Puede ser nulo.
    /// </summary>
    public string? ApellidoMaterno
    {
        get => field;
        set => field = value; // No se requiere validación específica, puede ser nulo
    }

    /// <summary>
    /// Número de teléfono del cliente. Puede ser nulo.
    /// </summary>
    public string? Telefono
    {
        get => field;
        set => field = value; // Se podría añadir validación de formato si es necesario
    }

    /// <summary>
    /// Correo electrónico del cliente. No puede ser nulo ni estar vacío y debe tener un formato válido.
    /// </summary>
    public string Email
    {
        get => field;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El correo electrónico del cliente no puede estar vacío.", nameof(Email));
            if (!IsValidEmail(value))
                throw new ArgumentException("El formato del correo electrónico no es válido.", nameof(Email));
            field = value;
        }
    }

    /// <summary>
    /// Dirección del cliente. Puede ser nula.
    /// </summary>
    public string? Direccion
    {
        get => field;
        set => field = value;
    }

    /// <summary>
    /// Fecha de registro del cliente.
    /// </summary>
    public DateTime FechaRegistro
    {
        get => field;
        private set => field = value;
    }

    /// <summary>
    /// Indica si el cliente está activo.
    /// </summary>
    public bool Activo
    {
        get => field;
        private set => field = value;
    }

    /// <summary>
    /// Activa el cliente.
    /// </summary>
    public void Activate()
    {
        Activo = true;
    }

    /// <summary>
    /// Desactiva el cliente.
    /// </summary>
    public void Deactivate()
    {
        Activo = false;
    }

    /// <summary>
    /// Valida el formato básico de un correo electrónico.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
