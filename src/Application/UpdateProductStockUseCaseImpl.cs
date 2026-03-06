using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Repositories;
using Utm_market.Core.UseCases;
using System; // For ArgumentOutOfRangeException

namespace Utm_market.Application;

public class UpdateProductStockUseCaseImpl(IProductRepository productRepository) : IUpdateProductStockUseCase
{
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<bool> ExecuteAsync(int productId, int newStock, CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(productId), "Product ID must be positive.");
        }
        if (newStock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(newStock), "New stock cannot be negative.");
        }

        return await _productRepository.UpdateStockAsync(productId, newStock, cancellationToken);
    }
}
