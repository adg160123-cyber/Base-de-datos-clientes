using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;
using Utm_market.Core.Repositories;
using Utm_market.Core.UseCases;
using System; // For ArgumentOutOfRangeException

namespace Utm_market.Application;

public class FetchSaleByIdUseCaseImpl(ISaleRepository saleRepository) : IFetchSaleByIdUseCase
{
    private readonly ISaleRepository _saleRepository = saleRepository;

    public async Task<Sale?> ExecuteAsync(int saleId, CancellationToken cancellationToken = default)
    {
        if (saleId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(saleId), "Sale ID must be positive.");
        }

        return await _saleRepository.GetByIdAsync(saleId, cancellationToken);
    }
}
