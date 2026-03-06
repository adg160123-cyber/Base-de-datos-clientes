using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;

namespace Utm_market.Core.UseCases;

/// <summary>
/// Defines the contract for updating only the status of an existing sale.
/// </summary>
public interface IUpdateSaleStatusUseCase
{
    /// <summary>
    /// Executes the use case to update the status of a specific sale.
    /// </summary>
    /// <param name="saleId">The unique identifier of the sale to update.</param>
    /// <param name="newStatus">The new <see cref="SaleStatus"/> for the sale.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the sale status was updated successfully, false otherwise (e.g., sale not found).</returns>
    Task<bool> ExecuteAsync(int saleId, SaleStatus newStatus, CancellationToken cancellationToken = default);
}
