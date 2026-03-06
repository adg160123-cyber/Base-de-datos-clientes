// src/Core/UseCases/IUpdateProductUseCase.cs

using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;

namespace Utm_market.Core.UseCases;

/// <summary>
/// Define el contrato para el caso de uso que actualiza un producto existente.
/// </summary>
public interface IUpdateProductUseCase
{
    /// <summary>
    /// Ejecuta el caso de uso para actualizar un producto.
    /// </summary>
    /// <param name="product">El objeto <see cref="Product"/> con los datos actualizados.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una <see cref="Task{bool}"/> que indica si la actualización fue exitosa.</returns>
    Task<bool> ExecuteAsync(Product product, CancellationToken cancellationToken = default);
}
