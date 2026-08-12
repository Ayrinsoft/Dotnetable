using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class RecordAttachmentService : IRecordAttachmentService
{
    private readonly AppDbContext _context;

    public RecordAttachmentService(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<RecordAttachmentDto>> ListAsync(string entityType, long entityId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(entityType) || entityId <= 0)
            return Array.Empty<RecordAttachmentDto>();

        var type = entityType.Trim();
        var rows = await _context.RecordAttachments.AsNoTracking()
            .Include(a => a.FileRecord)
            .Where(a => a.EntityType == type && a.EntityID == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        return rows.Select(Map).ToList();
    }

    public async Task<(bool Success, string? Error, RecordAttachment? Attachment)> AttachAsync(
        int websiteId, string entityType, long entityId, int fileRecordId,
        string? title, string? note, int? memberId, CancellationToken ct = default)
    {
        if (websiteId <= 0) return (false, "Website is required.", null);
        if (string.IsNullOrWhiteSpace(entityType)) return (false, "Entity type is required.", null);
        if (entityId <= 0) return (false, "Entity id is required.", null);
        if (fileRecordId <= 0) return (false, "File is required.", null);

        var type = entityType.Trim();
        if (type.Length > 64) return (false, "Entity type is too long.", null);

        var file = await _context.FileRecords.AsNoTracking()
            .FirstOrDefaultAsync(f => f.FileRecordID == fileRecordId && !f.IsDeleted, ct);
        if (file is null) return (false, "File not found.", null);
        if (file.WebsiteID != websiteId)
            return (false, "File belongs to a different website.", null);

        var exists = await _context.RecordAttachments.AnyAsync(
            a => a.EntityType == type && a.EntityID == entityId && a.FileRecordID == fileRecordId, ct);
        if (exists) return (false, "This file is already attached.", null);

        var attachment = new RecordAttachment
        {
            WebsiteID = websiteId,
            EntityType = type,
            EntityID = entityId,
            FileRecordID = fileRecordId,
            Title = Truncate(title, 200) ?? Truncate(file.Title ?? file.OriginalFileName, 200),
            Note = Truncate(note, 500),
            CreatedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
        };
        _context.RecordAttachments.Add(attachment);
        await _context.SaveChangesAsync(ct);
        return (true, null, attachment);
    }

    public async Task<(bool Success, string? Error)> RemoveAsync(long attachmentId, int? memberId, CancellationToken ct = default)
    {
        var row = await _context.RecordAttachments.FirstOrDefaultAsync(a => a.RecordAttachmentID == attachmentId, ct);
        if (row is null) return (false, "Attachment not found.");
        _context.RecordAttachments.Remove(row);
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    private static RecordAttachmentDto Map(RecordAttachment a) => new()
    {
        RecordAttachmentID = a.RecordAttachmentID,
        EntityType = a.EntityType,
        EntityID = a.EntityID,
        FileRecordID = a.FileRecordID,
        Title = a.Title,
        Note = a.Note,
        FileName = a.FileRecord?.OriginalFileName,
        MimeType = a.FileRecord?.MimeType,
        Url = a.FileRecord?.CNDUrl,
        ThumbnailUrl = a.FileRecord?.ThumbnailCDN,
        CreatedAt = a.CreatedAt,
        CreatedByMemberID = a.CreatedByMemberID,
    };

    private static string? Truncate(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var t = value.Trim();
        return t.Length <= max ? t : t[..max];
    }
}
