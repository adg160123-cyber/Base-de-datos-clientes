// src/Core/Filters/ClientFilter.cs

namespace Utm_market.Core.Filters;

/// <summary>
/// Representa un objeto de filtro inmutable para búsquedas de <see cref="Client"/> en la capa de dominio.
/// Diseñado como un record para inmutabilidad y concisión, compatible con Native AOT.
/// </summary>
/// <param name="Nombre">Filtra por el nombre del cliente (búsqueda parcial insensible a mayúsculas/minúsculas).</param>
/// <param name="ApellidoPaterno">Filtra por el apellido paterno del cliente (búsqueda parcial insensible a mayúsculas/minúsculas).</param>
/// <param name="Email">Filtra por el correo electrónico del cliente (búsqueda parcial insensible a mayúsculas/minúsculas).</param>
/// <param name="Activo">Filtra por el estado activo del cliente. Si es null, no se aplica filtro por estado.</param>
public record ClientFilter(string? Nombre = null, string? ApellidoPaterno = null, string? Email = null, bool? Activo = null);
