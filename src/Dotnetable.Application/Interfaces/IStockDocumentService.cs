using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

public sealed class StockDocumentLineRequest
{
    public int ProductVariantID { get; set; }
    /// <summary>Movement qty (Transfer/Inbound/…) or counted qty when creating Count lines.</summary>
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Note { get; set; }
    /// <summary><see cref="StockItemCondition"/> for return docs (default New when type is Return).</summary>
    public byte ReturnCondition { get; set; }
    /// <summary><see cref="StockHealthGrade"/> required for non-new conditions.</summary>
    public byte HealthGrade { get; set; }
    /// <summary>Optional book snapshot for Count (filled by server if 0).</summary>
    public int BookQuantity { get; set; }
    /// <summary>Optional counted qty for Count (defaults to Quantity when creating Count).</summary>
    public int? CountedQuantity { get; set; }
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
    Task<PagedResult<StockDocument>> GetPagedAsync(int websiteId, byte? status, byte? type, GridQuery query, CancellationToken ct = default, bool excludeReturns = false);
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
    /// Registers a customer return (RMA) from an order <em>before</em> refund. Draft Return at the QC warehouse.
    /// Fails when the order already has an open (not posted/cancelled) return.
    /// </summary>
    Task<(bool Success, string? Error, StockDocument? Doc)> CreateCustomerReturnAsync(
        int orderId, int? toWarehouseId, IReadOnlyList<StockDocumentLineRequest>? lines, string? note, int? memberId,
        CancellationToken ct = default);

    /// <summary>Change receive / restock warehouse on a return that is not yet posted.</summary>
    Task<(bool Success, string? Error)> SetDestinationWarehouseAsync(
        int documentId, int toWarehouseId, int? memberId, CancellationToken ct = default);

    /// <summary>
    /// After refund when goods already left stock: create Submitted Return doc for QC + restock.
    /// Idempotent per refund / order return. Links the refund onto an existing RMA when one exists.
    /// </summary>
    Task<(bool Success, string? Error, StockDocument? Doc)> EnsureReturnForRefundAsync(
        int orderId, int paymentRefundId, int? memberId, CancellationToken ct = default);

    /// <summary>Update item condition + health grade on a return line before post.</summary>
    Task<(bool Success, string? Error)> SetReturnLineConditionAsync(
        int stockDocumentLineId, StockItemCondition condition, StockHealthGrade healthGrade, int? memberId, CancellationToken ct = default);

    /// <summary>Worker queue: outbound docs in Submitted or Approved (ready to pick).</summary>
    Task<IReadOnlyList<WarehousePickTaskDto>> GetPickQueueAsync(
        int websiteId, byte? statusFilter, CancellationToken ct = default);

    /// <summary>Submitted → Approved (Ready to pick).</summary>
    Task<(bool Success, string? Error)> MarkReadyToPickAsync(int documentId, int? memberId, CancellationToken ct = default);

    /// <summary>
    /// Creates a Draft Count document pre-filled from warehouse on-hand (book = on-hand, counted = book).
    /// </summary>
    Task<(bool Success, string? Error, StockDocument? Doc)> CreateCountFromWarehouseAsync(
        int websiteId, int warehouseId, string? note, int? memberId, CancellationToken ct = default);

    /// <summary>Update counted qty on a Count line (recomputes variance into Quantity).</summary>
    Task<(bool Success, string? Error)> SetCountedQuantityAsync(
        int stockDocumentLineId, int countedQuantity, int? memberId, CancellationToken ct = default);

    /// <summary>Add a line to a Draft document (Transfer multi-line etc.).</summary>
    Task<(bool Success, string? Error)> AddLineAsync(
        int documentId, StockDocumentLineRequest line, int? memberId, CancellationToken ct = default);
}
