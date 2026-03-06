using System.Collections.Generic;
using System.Threading;
using Utm_market.Core.Entities;
using Utm_market.Core.Filters;
using Utm_market.Core.Repositories;
using Utm_market.Core.UseCases;
using System.Runtime.CompilerServices; // For [EnumeratorCancellation]
using System; // For ArgumentNullException

namespace Utm_market.Application;

public class FetchSalesByFilterUseCaseImpl(ISaleRepository saleRepository) : IFetchSalesByFilterUseCase
{
    private readonly ISaleRepository _saleRepository = saleRepository;

    public async IAsyncEnumerable<Sale> ExecuteAsync(
        SaleFilter filter,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        await foreach (var sale in _saleRepository.FindAsync(filter, cancellationToken))
        {
            yield return sale;
        }
    }
}
