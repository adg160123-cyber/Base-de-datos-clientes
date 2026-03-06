using System.Collections.Generic;
using System.Threading;
using Utm_market.Core.Entities;
using Utm_market.Core.Repositories;
using Utm_market.Core.UseCases;
using System.Runtime.CompilerServices; // For [EnumeratorCancellation]
using System; // For ArgumentNullException

namespace Utm_market.Application;

public class FetchAllSalesUseCaseImpl(ISaleRepository saleRepository) : IFetchAllSalesUseCase
{
    private readonly ISaleRepository _saleRepository = saleRepository;

    public async IAsyncEnumerable<Sale> ExecuteAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var sale in _saleRepository.GetAllAsync(cancellationToken))
        {
            yield return sale;
        }
    }
}
