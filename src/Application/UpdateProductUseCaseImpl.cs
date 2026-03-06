using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;
using Utm_market.Core.Repositories;
using Utm_market.Core.UseCases;
using System; // For ArgumentNullException

namespace Utm_market.Application;

public class UpdateProductUseCaseImpl(IProductRepository productRepository) : IUpdateProductUseCase
{
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<bool> ExecuteAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        if (product.ProductID <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(product.ProductID), "Product ID must be positive for update.");
        }

        return await _productRepository.UpdateAsync(product, cancellationToken);
    }
}
