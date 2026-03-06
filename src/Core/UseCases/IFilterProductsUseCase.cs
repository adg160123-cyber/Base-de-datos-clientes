// src/Core/UseCases/IFilterProductsUseCase.cs

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;
using Utm_market.Core.Filters;

namespace Utm_market.Core.UseCases;

/// <summary>
/// Define el contrato para el caso de uso que filtra productos basándose en criterios específicos.
/// Permite el procesamiento en streaming de los resultados filtrados.
/// </summary>
public interface IFilterProductsUseCase
{
    /// <summary>
    /// Ejecuta el caso de uso para filtrar productos.
    /// </summary>
    /// <param name="filter">Objeto <see cref="ProductFilter"/> con los criterios de búsqueda.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Un <see cref="IAsyncEnumerable{Product}"/> que representa un flujo de productos que coinciden con el filtro.</returns>
    IAsyncEnumerable<Product> ExecuteAsync(ProductFilter filter, CancellationToken cancellationToken = default);
}
