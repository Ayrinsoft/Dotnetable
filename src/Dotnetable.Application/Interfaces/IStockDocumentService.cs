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
    /// <summary><see cref="StockReturnCondition"/> for return docs (default Sellable when type is Return).</summary>
    public byte ReturnCondition { get; set; }
}

/// <summary>Row for the warehouse worker pick queue.</summary>
public sealed class WarehousePickTaskDto
{
    public int StockDocumentID { get; init; }
    public string DocumentNumber { get; init; } = "";
    public byte Status { get; init; }
    public int? OrderID { get; init; }
    public string? OrderNumber { get; init; }
    public int LineCount { get; init; }
    public int TotalQuantity { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public string? FromWarehouseName { get; init; }
    public string? Note { get; init; }
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

    /// <summary>
    /// Creates a Submitted outbound pick document from a paid order (physical lines only). Idempotent per order.
    /// </summary>
    Task<(bool Success, string? Error, StockDocument? Doc)> EnsureOutboundForOrderAsync(int orderId, int? memberId, CancellationToken ct = default);

    /// <summary>Approves (if needed) and posts the order's outbound document — stock leaves warehouse.</summary>
    Task<(bool Success, string? Error)> PostOutboundForOrderAsync(int orderId, int? memberId, CancellationToken ct = default);

    /// <summary>
    /// Preflight: enough warehouse on-hand (after reservations) for the order's physical lines.
    /// Returns (true, null) when WMS is not used or stock is sufficient.
    /// </summary>
    Task<(bool CanShip, string? Error)> CanShipOrderAsync(int orderId, CancellationToken ct = default);

    /// <summary>True when website has at least one active warehouse (WMS path for orders).</summary>
    Task<bool> WebsiteHasWarehouseAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Active (non-cancelled) outbound document linked to an order, if any.</summary>
    Task<StockDocument?> GetOutboundForOrderAsync(int orderId, CancellationToken ct = default);

    /// <summary>Active return document linked to an order, if any.</summary>
    Task<StockDocument?> GetReturnForOrderAsync(int orderId, CancellationToken ct = default);

    /// <summary>
    /// After refund when goods already left stock: create Submitted Return doc for QC + restock.
    /// Idempotent per refund / order return.
    /// </summary>
    Task<(bool Success, string? Error, StockDocument? Doc)> EnsureReturnForRefundAsync(
        int orderId, int paymentRefundId, int? memberId, CancellationToken ct = default);

    /// <summary>Update QC condition on a return line before post.</summary>
    Task<(bool Success, string? Error)> SetReturnLineConditionAsync(
        int stockDocumentLineId, StockReturnCondition condition, int? memberId, CancellationToken ct = default);

    /// <summary>Worker queue: outbound docs in Submitted or Approved (ready to pick).</summary>
    Task<IReadOnlyList<WarehousePickTaskDto>> GetPickQueueAsync(
        int websiteId, byte? statusFilter, CancellationToken ct = default);

    /// <summary>Submitted → Approved (Ready to pick).</summary>
    Task<(bool Success, string? Error)> MarkReadyToPickAsync(int documentId, int? memberId, CancellationToken ct = default);
}
