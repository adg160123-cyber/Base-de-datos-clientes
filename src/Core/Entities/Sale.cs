// src/Core/Entities/Sale.cs
using System.Collections.Generic;
using System.Linq;

namespace Utm_market.Core.Entities;

public class Sale(
    int saleId,
    string folio,
    SaleStatus status)
{
    public int SaleID { get; init; } = saleId;
    public string Folio { get; init; } = folio;
    public DateTime SaleDate { get; init; } = DateTime.Now; // Automatically initialized
    public SaleStatus Status { get; init; } = status;

    private readonly List<SaleDetail> _saleDetailsField = new(); // C# 14 'field' keyword
    public IReadOnlyCollection<SaleDetail> SaleDetails
    {
        get => _saleDetailsField.AsReadOnly();
        init => _saleDetailsField = new List<SaleDetail>(value); // Allows initialization with a collection
    }

    public void AddSaleDetail(SaleDetail detail)
    {
        if (detail == null)
        {
            throw new ArgumentNullException(nameof(detail));
        }
        _saleDetailsField.Add(detail);
    }

    // Calculated property for total items using expression-bodied member
    public int TotalItems => SaleDetails.Sum(detail => detail.Quantity);

    // Calculated property for total sale using expression-bodied member
    public decimal TotalSale => SaleDetails.Sum(detail => detail.TotalDetail);
}
