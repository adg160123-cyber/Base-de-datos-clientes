// src/Infrastructure/Models/Data/VentaEntity.cs
using System;

namespace Utm_market.Infrastructure.Models.Data;

public partial class VentaEntity(
    int ventaId,
    string folio)
{
    public int VentaID { get; init; } = ventaId;
    public string Folio { get; init; } = folio;
    public DateTime FechaVenta { get; init; } = DateTime.Now; // DEFAULT GETDATE() in SQL

    private int _totalArticulosField;
    public int TotalArticulos
    {
        get => _totalArticulosField;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(TotalArticulos), "TotalArticulos cannot be negative.");
            }
            _totalArticulosField = value;
        }
    }

    private decimal _totalVentaField;
    public decimal TotalVenta
    {
        get => _totalVentaField;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(TotalVenta), "TotalVenta cannot be negative.");
            }
            _totalVentaField = value;
        }
    }

    private byte _estatusField; // TINYINT in SQL, mapping to byte
    public byte Estatus
    {
        get => _estatusField;
        init
        {
            // CHECK ([Estatus] IN (1, 2, 3))
            if (value != 1 && value != 2 && value != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Estatus), "Estatus must be 1 (Pendiente), 2 (Completada), or 3 (Cancelada).");
            }
            _estatusField = value;
        }
    }
}
