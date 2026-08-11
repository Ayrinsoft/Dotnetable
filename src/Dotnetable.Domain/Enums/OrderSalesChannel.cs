namespace Dotnetable.Domain.Enums;

/// <summary>
/// Where the order originated. Online = storefront checkout; other values are typically admin-entered
/// social / offline sales (Instagram, WhatsApp, etc.).
/// Stored on <c>Order.SalesChannel</c> as TINYINT.
/// </summary>
public enum OrderSalesChannel : byte
{
    Online = 1,
    Instagram = 2,
    WhatsApp = 3,
    Telegram = 4,
    Phone = 5,
    InPerson = 6,
    Other = 99,
}
