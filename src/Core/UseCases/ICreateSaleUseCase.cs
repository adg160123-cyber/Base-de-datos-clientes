using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;

namespace Utm_market.Core.UseCases;

/// <summary>
/// Defines the contract for creating and persisting a new sale.
/// </summary>
public interface ICreateSaleUseCase
{
    /// <summary>
    /// Executes the use case to create a new sale.
    /// The input <see cref="Sale"/> aggregate (with its details) will be persisted.
    /// </summary>
    /// <param name="sale">The <see cref="Sale"/> aggregate to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created <see cref="Sale"/> aggregate, including its database-generated ID and details.</returns>
    Task<Sale> ExecuteAsync(Sale sale, CancellationToken cancellationToken = default);
}
