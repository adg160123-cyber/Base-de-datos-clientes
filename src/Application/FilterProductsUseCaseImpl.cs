using System.Collections.Generic;
using System.Threading;
using Utm_market.Core.Entities;
using Utm_market.Core.Filters;
using Utm_market.Core.Repositories;
using Utm_market.Core.UseCases;
using System.Runtime.CompilerServices; // For [EnumeratorCancellation]
using System; // For ArgumentNullException

namespace Utm_market.Application;

public class FilterProductsUseCaseImpl(IProductRepository productRepository) : IFilterProductsUseCase
{
    private readonly IProductRepository _productRepository = productRepository;

    public async IAsyncEnumerable<Product> ExecuteAsync(
        ProductFilter filter,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        await foreach (var product in _productRepository.FindAsync(filter, cancellationToken))
        {
            yield return product;
        }
    }
}
