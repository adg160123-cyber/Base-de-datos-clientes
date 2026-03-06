using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;

namespace Utm_market.Core.UseCases;

/// <summary>
/// Defines the contract for fetching a specific sale by its unique identifier.
/// </summary>
public interface IFetchSaleByIdUseCase
{
    /// <summary>
    /// Executes the use case to retrieve a sale by its ID.
    /// The returned <see cref="Sale"/> should include its <see cref="SaleDetail"/>s.
    /// </summary>
    /// <param name="saleId">The unique identifier of the sale.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Sale"/> entity if found, otherwise null.</returns>
    Task<Sale?> ExecuteAsync(int saleId, CancellationToken cancellationToken = default);
}
