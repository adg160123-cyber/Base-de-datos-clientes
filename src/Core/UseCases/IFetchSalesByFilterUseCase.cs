using System.Collections.Generic;
using System.Threading;
using Utm_market.Core.Entities;
using Utm_market.Core.Filters;

namespace Utm_market.Core.UseCases;

/// <summary>
/// Defines the contract for fetching sales based on specified filter criteria.
/// </summary>
public interface IFetchSalesByFilterUseCase
{
    /// <summary>
    /// Executes the use case to retrieve sales matching the provided filter.
    /// Each returned <see cref="Sale"/> should include its <see cref="SaleDetail"/>s.
    /// </summary>
    /// <param name="filter">The <see cref="SaleFilter"/> object containing the search criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An asynchronous enumerable of <see cref="Sale"/> entities matching the filter.</returns>
    IAsyncEnumerable<Sale> ExecuteAsync(SaleFilter filter, CancellationToken cancellationToken = default);
}
