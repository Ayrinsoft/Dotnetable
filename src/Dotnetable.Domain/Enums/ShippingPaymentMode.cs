namespace Dotnetable.Domain.Enums;

/// <summary>
/// How shipping is paid relative to delivery.
/// Prepaid = charged with the order. CashOnDelivery = paid on delivery (postpay).
/// </summary>
public enum ShippingPaymentMode : byte
{
    Prepaid = 0,
    CashOnDelivery = 1,
}
