namespace Dotnetable.Domain.Enums;

/// <summary>
/// How shipping is paid relative to delivery.
/// Prepaid = پیش‌کرایه (paid with the order). COD = پس‌کرایه (paid on delivery).
/// </summary>
public enum ShippingPaymentMode : byte
{
    Prepaid = 0,
    CashOnDelivery = 1,
}
