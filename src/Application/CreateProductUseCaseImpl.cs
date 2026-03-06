using System.Threading;
using System.Threading.Tasks;
using Utm_market.Core.Entities;
using Utm_market.Core.Repositories;
using Utm_market.Core.UseCases;
using System; // For ArgumentNullException

namespace Utm_market.Application;

public class CreateProductUseCaseImpl(IProductRepository productRepository) : ICreateProductUseCase
{
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<Product> ExecuteAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        // Assumption: IProductRepository.AddAsync now returns Task<Product> as suggested in previous output.
        // If it still returns Task, then a subsequent GetByIdAsync would be needed,
        // which would be less efficient and potentially problematic with eventual consistency.
        return await _productRepository.AddAsync(product, cancellationToken);
    }
}
