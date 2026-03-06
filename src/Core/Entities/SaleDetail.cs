// src/Core/Entities/SaleDetail.cs
namespace Utm_market.Core.Entities;

public class SaleDetail(
    int saleDetailId,
    int productId,
    int quantity,
    decimal unitPrice)
{
    public int SaleDetailID { get; init; } = saleDetailId;
    public int ProductID { get; init; } = productId; // Reference to Product ID

    private int _quantityField = quantity;
    public int Quantity
    {
        get => _quantityField;
        init
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Quantity), "Quantity must be positive.");
            }
            _quantityField = value;
        }
    }

    public decimal UnitPrice { get; init; } = unitPrice; // Price at the time of sale, independent of current product price

    // Calculated property using expression-bodied member
    public decimal TotalDetail => UnitPrice * Quantity;
}
