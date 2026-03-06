// src/Core/UseCases/IUpdateProductStockUseCase.cs

using System.Threading;
using System.Threading.Tasks;

namespace Utm_market.Core.UseCases;

/// <summary>
/// Define el contrato para el caso de uso que actualiza el stock de un producto.
/// </summary>
public interface IUpdateProductStockUseCase
{
    /// <summary>
    /// Ejecuta el caso de uso para actualizar el stock de un producto específico.
    /// </summary>
    /// <param name="productId">El identificador único del producto.</param>
    /// <param name="newStock">La nueva cantidad de stock para el producto. Debe ser no negativa.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una <see cref="Task{bool}"/> que indica si la actualización fue exitosa.</returns>
    Task<bool> ExecuteAsync(int productId, int newStock, CancellationToken cancellationToken = default);
}
