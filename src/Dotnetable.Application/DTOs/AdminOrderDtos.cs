using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

/// <summary>One line on an admin-created order (catalog variant and/or free-form title).</summary>
public sealed class AdminOrderLineRequest
{
    /// <summary>
    /// Catalog variant to sell. Null when the line is free-form (<see cref="CustomTitle"/>).
    /// </summary>
    public int? ProductVariantId { get; set; }

    /// <summary>
    /// Free-form line title when there is no catalog product (Instagram custom item, etc.).
    /// Ignored when <see cref="ProductVariantId"/> is set (title is snapshotted from the variant).
    /// </summary>
    public string? CustomTitle { get; set; }

    /// <summary>Optional SKU label for free-form lines.</summary>
    public string? CustomSku { get; set; }

    /// <summary>
    /// Optional store listing. When null and a variant is set, the service picks the best active
    /// listing for the host website; if none exists the line is still accepted without stock tracking.
    /// </summary>
    public int? VendorProductId { get; set; }

    public int Quantity { get; set; } = 1;

    /// <summary>Unit price charged to the customer (order / site currency).</summary>
    public decimal ChargedUnitPrice { get; set; }

    /// <summary>
    /// Optional override of the catalog unit price used to compute instant markup.
    /// When null, the current catalog price is used (or charged price for free-form lines).
    /// </summary>
    public decimal? CatalogUnitPrice { get; set; }
}

/// <summary>Optional payment recorded when the admin creates the order (card-to-card, cash, …).</summary>
public sealed class AdminOrderPaymentRequest
{
    /// <summary>
    /// Supported: <c>BankTransfer</c> (card-to-card with optional receipt), <c>Manual</c>, <c>CashOnDelivery</c>.
    /// </summary>
    public byte Method { get; set; }

    /// <summary>Amount in order currency; defaults to grand total when null or ≤ 0.</summary>
    public decimal? Amount { get; set; }

    public string? Reference { get; set; }

    public string? Note { get; set; }

    /// <summary>Required for bank transfer when recording a destination account.</summary>
    public int? BankAccountId { get; set; }

    /// <summary>Uploaded receipt file id (card-to-card / bank transfer).</summary>
    public int? ReceiptFileId { get; set; }

    /// <summary>
    /// When true (default), payment is stored as Paid and the order transitions PendingPayment → Paid.
    /// When false for bank transfer, payment stays Pending for the verification queue.
    /// </summary>
    public bool MarkAsPaid { get; set; } = true;

    /// <summary>
    /// When the payment was received (UTC). Required for paid recordings; when null, UtcNow is used.
    /// </summary>
    public DateTime? PaidAtUtc { get; set; }
}

/// <summary>Optional shipping address payload when not using an existing client address id.</summary>
public sealed class AdminOrderAddressRequest
{
    public string? Title { get; set; }
    public string? ReceiverName { get; set; }
    public int? CountryId { get; set; }
    public int? CityId { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public bool SaveToClient { get; set; } = true;
}

/// <summary>Admin creates a full order for a website customer without storefront checkout.</summary>
public sealed class AdminCreateOrderRequest
{
    public int WebsiteId { get; set; }
    public int WebsiteClientId { get; set; }

    /// <summary>Existing saved address; mutually exclusive with <see cref="NewAddress"/> when both are usable.</summary>
    public int? WebsiteClientAddressId { get; set; }

    /// <summary>Create / snapshot a new address when no address id is provided (or to save a new one).</summary>
    public AdminOrderAddressRequest? NewAddress { get; set; }

    public int? ShippingMethodId { get; set; }

    /// <summary>Override shipping charge in site currency; when null, quote from shipping service when possible.</summary>
    public decimal? ShippingTotal { get; set; }

    public OrderSalesChannel SalesChannel { get; set; } = OrderSalesChannel.Instagram;

    /// <summary>
    /// When null, derived from channel + website setting:
    /// Online → true; other channels → <c>Website.ReportOfflineOrdersToTax</c>.
    /// </summary>
    public bool? ReportToTax { get; set; }

    public string? Note { get; set; }

    public string? CurrencyCode { get; set; }

    public List<AdminOrderLineRequest> Lines { get; set; } = new();

    public AdminOrderPaymentRequest? Payment { get; set; }
}

/// <summary>Result of <c>IOrderService.AdminCreateAsync</c>.</summary>
public sealed record AdminCreateOrderResult(
    bool Success,
    string? Error,
    int? OrderId,
    string? OrderNumber);
