// src/Core/Entities/SaleStatus.cs
namespace Utm_market.Core.Entities;

public enum SaleStatus
{
    /// <summary>
    /// The sale has been initiated but not yet completed.
    /// </summary>
    Pending,

    /// <summary>
    /// The sale has been successfully completed.
    /// </summary>
    Completed,

    /// <summary>
    /// The sale has been cancelled.
    /// </summary>
    Cancelled,

    /// <summary>
    /// The sale is awaiting payment.
    /// </summary>
    AwaitingPayment
}
