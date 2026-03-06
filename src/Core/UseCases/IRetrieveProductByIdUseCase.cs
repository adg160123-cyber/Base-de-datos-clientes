// src/Core/UseCases/IRetrieveProductByIdUseCase.cs

using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;

namespace Utm_market.Core.UseCases;

/// <summary>
/// Define el contrato para el caso de uso que recupera un producto por su identificador único.
/// </summary>
public interface IRetrieveProductByIdUseCase
{
    /// <summary>
    /// Ejecuta el caso de uso para obtener un producto específico.
    /// </summary>
    /// <param name="productId">El identificador único del producto a recuperar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una <see cref="Task{Product}"/> que contendrá el producto encontrado, o null si no existe.</returns>
    Task<Product?> ExecuteAsync(int productId, CancellationToken cancellationToken = default);
}
