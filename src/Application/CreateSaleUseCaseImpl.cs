using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;
using Utm_market.Core.Repositories;
using Utm_market.Core.UseCases;
using System; // For ArgumentNullException

namespace Utm_market.Application;

public class CreateSaleUseCaseImpl(ISaleRepository saleRepository) : ICreateSaleUseCase
{
    private readonly ISaleRepository _saleRepository = saleRepository;

    public async Task<Sale> ExecuteAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sale);

        // Additional validation specific to creating a sale could go here, e.g.,
        // ensuring product IDs in SaleDetails exist, quantities are valid, etc.
        // For this task, we assume the Sale object is well-formed.

        return await _saleRepository.AddAsync(sale, cancellationToken);
    }
}
