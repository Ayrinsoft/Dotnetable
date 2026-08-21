using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Site-wide financial ledger: cash movements + analytical components for reporting.
/// Transaction types are free-form strings; versioned rows preserve edit history.
/// </summary>
public interface IFinancialLedgerService
{
    Task<PagedResult<FinancialLedgerEntryDto>> GetPagedAsync(
        FinancialLedgerFilter filter, GridQuery query, CancellationToken ct = default);

    Task<IReadOnlyList<FinancialLedgerEntryDto>> GetByOrderAsync(
        int orderId, bool currentOnly = true, bool includeHistory = false, CancellationToken ct = default);

    Task<OrderFinancialSummaryDto?> GetOrderSummaryAsync(int orderId, CancellationToken ct = default);

    Task<IReadOnlyList<FinancialLedgerEntryDto>> GetVendorVisibleAsync(
        int vendorId, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default);

    Task<FinancialLedgerEntry> PostAsync(PostFinancialEntryRequest request, CancellationToken ct = default);

    /// <summary>
    /// Posts (or refreshes) full order breakdown after money is received:
    /// customer payment, shipping, markup, line revenue/cost/profit, tax/discount components.
    /// Idempotent for the same order+payment when current rows already match.
    /// </summary>
    Task PostOrderPaidBreakdownAsync(int orderId, int? paymentId, int? memberId, CancellationToken ct = default);

    /// <summary>Customer refund cash movement (component reverse is optional).</summary>
    Task PostCustomerRefundAsync(int orderId, int paymentId, decimal amount, string? note, int? memberId, CancellationToken ct = default);

    /// <summary>
    /// Posts site return-shipping expense and an analytical site P&amp;L row for a completed RMA.
    /// Idempotent per return request.
    /// </summary>
    Task PostCustomerReturnImpactAsync(
        int orderId, int customerReturnRequestId,
        decimal siteShippingShare, decimal siteImpactAmount,
        string currencyCode, string? note, int? vendorId, decimal sellerShippingShare,
        int? memberId, CancellationToken ct = default);

    /// <summary>
    /// Posts inventory COGS to L1 + GL when goods leave stock (WMS outbound post or non-WMS fulfill).
    /// Idempotent per stock document (or per order when <paramref name="stockDocumentId"/> is null).
    /// Analytical <c>OrderLineCost</c> at payment remains for product margins; GL COGS is recognized here so books match warehouse.
    /// </summary>
    Task PostInventoryCogsForOrderAsync(int orderId, int? stockDocumentId, int? memberId, CancellationToken ct = default);

    /// <summary>
    /// Reverses inventory COGS when a sellable return is posted (not defective scrap).
    /// Idempotent per return stock document.
    /// </summary>
    Task PostInventoryCogsReversalForReturnAsync(int orderId, int stockDocumentId, int? memberId, CancellationToken ct = default);

    /// <summary>Vendor settlement / payable amount (vendor-visible).</summary>
    Task PostVendorSettlementAsync(
        int websiteId, int? vendorId, int? settlementId, int? orderId, int? orderItemId,
        decimal amount, string currencyCode, decimal amountUsd, string title, bool reportToTax,
        int? memberId, CancellationToken ct = default);

    /// <summary>Admin registers an extra charge on an order (and optional payment).</summary>
    Task<(bool Success, string? Error, FinancialLedgerEntry? Entry)> PostAdditionalChargeAsync(
        PostAdditionalChargeRequest request, CancellationToken ct = default);

    /// <summary>Edit a current row: marks old as historical and inserts a new version.</summary>
    Task<(bool Success, string? Error, FinancialLedgerEntry? Entry)> SupersedeAsync(
        long entryId, decimal newAmount, string? newTitle, string? newDescription,
        bool? reportToTax, string changeNote, int? memberId, CancellationToken ct = default);
}
