// src/Infrastructure/Models/Data/ProductoEntity.cs
using System;

namespace Utm_market.Infrastructure.Models.Data;

public partial class ProductoEntity(
    int productoId,
    string nombre,
    string sku)
{
    public int ProductoID { get; init; } = productoId;
    public string Nombre { get; init; } = nombre;
    public string SKU { get; init; } = sku;

    public string? Marca { get; init; }

    private decimal _precioField;
    public decimal Precio
    {
        get => _precioField;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Precio), "Precio cannot be negative.");
            }
            _precioField = value;
        }
    }

    private int _stockField;
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
