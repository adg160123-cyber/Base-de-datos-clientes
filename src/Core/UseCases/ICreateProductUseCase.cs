// src/Core/UseCases/ICreateProductUseCase.cs

using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;

namespace Utm_market.Core.UseCases;

/// <summary>
/// Define el contrato para el caso de uso que crea un nuevo producto.
/// </summary>
public interface ICreateProductUseCase
{
    /// <summary>
    /// Ejecuta el caso de uso para crear un nuevo producto.
    /// </summary>
    /// <param name="product">El objeto <see cref="Product"/> a crear.
    /// Su <see cref="Product.ProductID"/> será actualizado con el ID generado por la persistencia.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una <see cref="Task{Product}"/> que contendrá el producto creado con su ID asignado.</returns>
    Task<Product> ExecuteAsync(Product product, CancellationToken cancellationToken = default);
}
