using System.Collections.Generic;
using System.Threading;
using Utm_market.Core.Entities;
using Utm_market.Core.Repositories;
using Utm_market.Core.UseCases;
using System.Runtime.CompilerServices; // For [EnumeratorCancellation]
using System; // For ArgumentNullException

namespace Utm_market.Application;

public class RetrieveAllProductsUseCaseImpl(IProductRepository productRepository) : IRetrieveAllProductsUseCase
{
    private readonly IProductRepository _productRepository = productRepository;

    public async IAsyncEnumerable<Product> ExecuteAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var product in _productRepository.GetAllAsync(cancellationToken))
        {
            yield return product;
        }
    }
}
