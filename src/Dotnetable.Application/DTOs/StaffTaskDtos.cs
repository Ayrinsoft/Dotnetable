using Dotnetable.Application.RecordAttachments;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

public sealed class StaffTaskDto
{
    public int StaffTaskID { get; init; }
    public int WebsiteID { get; init; }
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public byte Status { get; init; }
    public byte Priority { get; init; }
    public int AssignedMemberID { get; init; }
    public string AssignedMemberName { get; init; } = "";
    public int CreatedByMemberID { get; init; }
    public string CreatedByMemberName { get; init; } = "";
    public DateTime? DueAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public byte RelatedKind { get; init; }
    public int? RelatedEntityID { get; init; }
    public string? RelatedLabel { get; init; }
    public string? RelatedUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int NoteCount { get; init; }
    public IReadOnlyList<StaffTaskNoteDto> Notes { get; init; } = Array.Empty<StaffTaskNoteDto>();
}

public sealed class StaffTaskNoteDto
{
    public int StaffTaskNoteID { get; init; }
    public int CreatedByMemberID { get; init; }
    public string CreatedByMemberName { get; init; } = "";
    public string Body { get; init; } = "";
    public DateTime CreatedAt { get; init; }
}

public sealed class StaffTaskColleagueDto
{
    public int MemberID { get; init; }
    public string Name { get; init; } = "";
    public string Username { get; init; } = "";
}

public sealed class StaffTaskRelatedHitDto
{
    public int Id { get; init; }
    public string Label { get; init; } = "";
}

public sealed class StaffTaskWriteRequest
{
    public int WebsiteID { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public byte Priority { get; set; }
    public int AssignedMemberID { get; set; }
    public DateTime? DueAt { get; set; }
    public byte RelatedKind { get; set; }
    public int? RelatedEntityID { get; set; }
}

public sealed record StaffTaskListFilter
{
    public int WebsiteID { get; init; }
    public int ActorMemberID { get; init; }
    public bool CanManage { get; init; }
    public byte? Status { get; init; }
    public int? AssignedMemberID { get; init; }
    public string? Search { get; init; }
}

public static class StaffTaskRelated
{
    public static string? EntityType(StaffTaskRelatedKind kind) => kind switch
    {
        StaffTaskRelatedKind.Order => RecordEntityTypes.Order,
        StaffTaskRelatedKind.StockInbound or StaffTaskRelatedKind.StockOutbound
            or StaffTaskRelatedKind.StockTransfer or StaffTaskRelatedKind.StockAdjustment
            or StaffTaskRelatedKind.StockCount or StaffTaskRelatedKind.StockReturn
            => RecordEntityTypes.StockDocument,
        StaffTaskRelatedKind.CustomerReturn => RecordEntityTypes.CustomerReturnRequest,
        StaffTaskRelatedKind.Payment => RecordEntityTypes.Payment,
        StaffTaskRelatedKind.PaymentRefund => RecordEntityTypes.PaymentRefund,
        _ => null,
    };

    public static byte? StockDocumentType(StaffTaskRelatedKind kind) => kind switch
    {
        StaffTaskRelatedKind.StockInbound => (byte)Domain.Enums.StockDocumentType.Inbound,
        StaffTaskRelatedKind.StockOutbound => (byte)Domain.Enums.StockDocumentType.Outbound,
        StaffTaskRelatedKind.StockTransfer => (byte)Domain.Enums.StockDocumentType.Transfer,
        StaffTaskRelatedKind.StockAdjustment => (byte)Domain.Enums.StockDocumentType.Adjustment,
        StaffTaskRelatedKind.StockCount => (byte)Domain.Enums.StockDocumentType.Count,
        StaffTaskRelatedKind.StockReturn => (byte)Domain.Enums.StockDocumentType.Return,
        _ => null,
    };

    public static string? Url(StaffTaskRelatedKind kind, int? relatedEntityId)
    {
        if (relatedEntityId is not int id || id <= 0) return null;
        return kind switch
        {
            StaffTaskRelatedKind.Order => $"/orders/{id}",
            StaffTaskRelatedKind.StockInbound or StaffTaskRelatedKind.StockOutbound
                or StaffTaskRelatedKind.StockTransfer or StaffTaskRelatedKind.StockAdjustment
                or StaffTaskRelatedKind.StockCount or StaffTaskRelatedKind.StockReturn
                => $"/inventory/stock-documents/{id}",
            StaffTaskRelatedKind.CustomerReturn => $"/inventory/returns/{id}",
            StaffTaskRelatedKind.Payment => "/payments",
            StaffTaskRelatedKind.PaymentRefund => "/payments/refunds",
            _ => null,
        };
    }

    public static StaffTaskRelatedKind FromStockDocumentType(byte documentType) => documentType switch
    {
        (byte)Domain.Enums.StockDocumentType.Inbound => StaffTaskRelatedKind.StockInbound,
        (byte)Domain.Enums.StockDocumentType.Outbound => StaffTaskRelatedKind.StockOutbound,
        (byte)Domain.Enums.StockDocumentType.Transfer => StaffTaskRelatedKind.StockTransfer,
        (byte)Domain.Enums.StockDocumentType.Adjustment => StaffTaskRelatedKind.StockAdjustment,
        (byte)Domain.Enums.StockDocumentType.Count => StaffTaskRelatedKind.StockCount,
        (byte)Domain.Enums.StockDocumentType.Return => StaffTaskRelatedKind.StockReturn,
        _ => StaffTaskRelatedKind.None,
    };
}
