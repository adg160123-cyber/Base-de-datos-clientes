// src/Core/UseCases/IRetrieveAllProductsUseCase.cs

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;

namespace Utm_market.Core.UseCases;

/// <summary>
/// Define el contrato para el caso de uso que recupera todos los productos.
/// Permite el procesamiento en streaming para optimizar el uso de memoria.
/// </summary>
public interface IRetrieveAllProductsUseCase
{
    /// <summary>
    /// Ejecuta el caso de uso para obtener todos los productos.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="IAsyncEnumerable{Product}"/> que representa un flujo de todos los productos.</returns>
    IAsyncEnumerable<Product> ExecuteAsync(CancellationToken cancellationToken = default);
}
