using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public sealed class RecordAttachmentDto
{
    public long RecordAttachmentID { get; init; }
    public string EntityType { get; init; } = "";
    public long EntityID { get; init; }
    public int FileRecordID { get; init; }
    public string? Title { get; init; }
    public string? Note { get; init; }
    public string? FileName { get; init; }
    public string? MimeType { get; init; }
    public string? Url { get; init; }
    public string? ThumbnailUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public int? CreatedByMemberID { get; init; }
}

public interface IRecordAttachmentService
{
    Task<IReadOnlyList<RecordAttachmentDto>> ListAsync(string entityType, long entityId, CancellationToken ct = default);

    Task<(bool Success, string? Error, RecordAttachment? Attachment)> AttachAsync(
        int websiteId, string entityType, long entityId, int fileRecordId,
        string? title, string? note, int? memberId, CancellationToken ct = default);

    Task<(bool Success, string? Error)> RemoveAsync(long attachmentId, int? memberId, CancellationToken ct = default);
}
