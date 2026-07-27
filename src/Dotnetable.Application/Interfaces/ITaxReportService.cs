using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

public interface ITaxReportService
{
    Task<VatReportDto> GetVatReportAsync(VatReportRequest request, CancellationToken ct = default);
    Task<OrderInvoiceDto?> GetOrderInvoiceAsync(int orderId, CancellationToken ct = default);
}
