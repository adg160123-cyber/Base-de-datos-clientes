using System.Collections.Generic;
using System.Threading;
using Utm_market.Core.Entities;

namespace Utm_market.Core.UseCases;

/// <summary>
/// Defines the contract for fetching all sales records.
/// </summary>
public interface IFetchAllSalesUseCase
{
    /// <summary>
    /// Executes the use case to retrieve all sales as an asynchronous collection.
    /// Each returned <see cref="Sale"/> should include its <see cref="SaleDetail"/>s.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An asynchronous enumerable of <see cref="Sale"/> entities.</returns>
    IAsyncEnumerable<Sale> ExecuteAsync(CancellationToken cancellationToken = default);
}
