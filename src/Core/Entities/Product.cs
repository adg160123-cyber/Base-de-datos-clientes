// src/Core/Entities/Product.cs
namespace Utm_market.Core.Entities;

public record class Product(
    int productId,
    string name,
    string sku,
    string brand,
    decimal price,
    int stock)
{
    public int ProductID { get; init; } = productId;
    public string Name { get; init; } = name;
    public string SKU { get; init; } = sku;
    public string Brand { get; init; } = brand;

    private decimal _priceField = price;
    public decimal Price
    {
        get => _priceField;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Price), "Price cannot be negative.");
            }
            _priceField = value;
        }
    }

    private int _stockField = stock;
    public int Stock
    {
        get => _stockField;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Stock), "Stock cannot be negative.");
            }
            _stockField = value;
        }
    }
}
