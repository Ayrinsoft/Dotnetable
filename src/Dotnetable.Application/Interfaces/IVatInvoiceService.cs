using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

public interface IVatInvoiceService
{
    /// <summary>Issues the sales VAT invoice for an order. A second call returns the invoice already issued.</summary>
    Task<VatInvoiceResult> IssueFromOrderAsync(int orderId, int? memberId, CancellationToken ct = default);

    /// <summary>Records a purchase VAT invoice received from a supplier.</summary>
    Task<VatInvoiceResult> ReceiveAsync(VatInvoiceReceiveWrite write, int? memberId, CancellationToken ct = default);

    Task<VatInvoiceDto?> GetAsync(int vatInvoiceId, CancellationToken ct = default);

    Task<VatInvoiceSummaryDto?> FindIssuedForOrderAsync(int orderId, CancellationToken ct = default);

    /// <summary>The issued invoice for an order that belongs to this customer.</summary>
    Task<VatInvoiceDto?> GetIssuedForClientAsync(int orderId, int clientId, CancellationToken ct = default);

    Task<IReadOnlyList<VatInvoiceSummaryDto>> ListAsync(VatInvoiceListQuery query, CancellationToken ct = default);
}
