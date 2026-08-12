using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

public sealed class StockDocumentLineRequest
{
    public int ProductVariantID { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Note { get; set; }
}

public interface IStockDocumentService
{
    Task<PagedResult<StockDocument>> GetPagedAsync(int websiteId, byte? status, byte? type, GridQuery query, CancellationToken ct = default);
    Task<StockDocument?> GetByIdAsync(int documentId, CancellationToken ct = default);
    Task<(bool Success, string? Error, StockDocument? Doc)> CreateAsync(
        int websiteId, StockDocumentType type, int? fromWarehouseId, int? toWarehouseId,
        int? supplierId, string? note, IReadOnlyList<StockDocumentLineRequest> lines, int? memberId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> SubmitAsync(int documentId, int? memberId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> ApproveAsync(int documentId, int? memberId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> RejectAsync(int documentId, int? memberId, string? note, CancellationToken ct = default);
    Task<(bool Success, string? Error)> PostAsync(int documentId, int? memberId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> CancelAsync(int documentId, int? memberId, string? note, CancellationToken ct = default);
}
