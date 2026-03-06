using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;
using Utm_market.Core.Repositories;
using Utm_market.Core.UseCases;
using System; // For ArgumentException

namespace Utm_market.Application;

public class RetrieveProductByIdUseCaseImpl(IProductRepository productRepository) : IRetrieveProductByIdUseCase
{
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<Product?> ExecuteAsync(int productId, CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(productId), "Product ID must be positive.");
        }

        return await _productRepository.GetByIdAsync(productId, cancellationToken);
    }
}
