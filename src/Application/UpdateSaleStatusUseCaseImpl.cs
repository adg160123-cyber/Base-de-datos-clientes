using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;
using Utm_market.Core.Repositories;
using Utm_market.Core.UseCases;
using System; // For ArgumentOutOfRangeException, ArgumentException

namespace Utm_market.Application;

public class UpdateSaleStatusUseCaseImpl(ISaleRepository saleRepository) : IUpdateSaleStatusUseCase
{
    private readonly ISaleRepository _saleRepository = saleRepository;

    public async Task<bool> ExecuteAsync(int saleId, SaleStatus newStatus, CancellationToken cancellationToken = default)
    {
        if (saleId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(saleId), "Sale ID must be positive.");
        }
        // Validate newStatus if there are specific rules, e.g., cannot go from Completed to Pending.
        // For now, assuming any valid SaleStatus enum value is acceptable.

        // Retrieve the sale aggregate
        var saleToUpdate = await _saleRepository.GetByIdAsync(saleId, cancellationToken);

        if (saleToUpdate == null)
        {
            return false; // Sale not found
        }

        // Check if the status is actually changing to avoid unnecessary updates
        if (saleToUpdate.Status == newStatus)
        {
            return true; // Status is already the desired state, consider it updated successfully.
        }
        
        // Create a new Sale instance with updated status due to immutability
        // Assuming Sale has a constructor or method to update status
        // If the Sale entity is truly immutable and has no way to change status after creation
        // without rebuilding the entire object, this approach might be problematic.
        // For this example, we assume we can create a new Sale instance with the updated status
        // and its existing details.
        var updatedSale = new Sale(
            saleId: saleToUpdate.SaleID,
            folio: saleToUpdate.Folio,
            status: newStatus)
        {
            SaleDate = saleToUpdate.SaleDate,
            SaleDetails = saleToUpdate.SaleDetails // Keep existing details
        };

        // Update the sale aggregate in the repository
        // As recommended in the previous architect_suggestions, a dedicated UpdateSaleStatusAsync
        // method in IProductRepository would be more efficient for this specific use case.
        return await _saleRepository.UpdateAsync(updatedSale, cancellationToken);
    }
}
