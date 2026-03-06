// src/Infrastructure/Models/Data/DetalleVentaEntity.cs
using System;

namespace Utm_market.Infrastructure.Models.Data;

public partial class DetalleVentaEntity(
    int detalleId,
    int ventaId,
    int productoId)
{
    public int DetalleID { get; init; } = detalleId;
    public int VentaID { get; init; } = ventaId;
    public int ProductoID { get; init; } = productoId;

    private decimal _precioUnitarioField;
    public decimal PrecioUnitario
    {
        get => _precioUnitarioField;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(PrecioUnitario), "PrecioUnitario cannot be negative.");
            }
            _precioUnitarioField = value;
        }
    }

    private int _cantidadField;
    public int Cantidad
    {
        get => _cantidadField;
        init
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Cantidad), "Cantidad must be positive.");
            }
            _cantidadField = value;
        }
    }

    private decimal _totalDetalleField;
    public decimal TotalDetalle
    {
        get => _totalDetalleField;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(TotalDetalle), "TotalDetalle cannot be negative.");
            }
            _totalDetalleField = value;
        }
    }
}
