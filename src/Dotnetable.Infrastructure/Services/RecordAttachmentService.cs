using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class RecordAttachmentService : IRecordAttachmentService
{
    private readonly IDbContextFactory<AppDbContext> _db;

    public RecordAttachmentService(IDbContextFactory<AppDbContext> db) => _db = db;

    public Task<IReadOnlyList<RecordAttachmentDto>> ListAsync(string entityType, long entityId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(entityType) || entityId <= 0)
            return Task.FromResult<IReadOnlyList<RecordAttachmentDto>>(Array.Empty<RecordAttachmentDto>());

        var type = entityType.Trim();
        return _db.UseAsync(async (context, token) =>
        {
            var rows = await context.RecordAttachments.AsNoTracking()
                .Include(a => a.FileRecord)
                .Where(a => a.EntityType == type && a.EntityID == entityId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(token);

            return (IReadOnlyList<RecordAttachmentDto>)rows.Select(Map).ToList();
        }, ct);
    }

    public Task<(bool Success, string? Error, RecordAttachment? Attachment)> AttachAsync(
        int websiteId, string entityType, long entityId, int fileRecordId,
        string? title, string? note, int? memberId, CancellationToken ct = default)
    {
        if (websiteId <= 0) return Task.FromResult<(bool, string?, RecordAttachment?)>((false, "Website is required.", null));
        if (string.IsNullOrWhiteSpace(entityType)) return Task.FromResult<(bool, string?, RecordAttachment?)>((false, "Entity type is required.", null));
        if (entityId <= 0) return Task.FromResult<(bool, string?, RecordAttachment?)>((false, "Entity id is required.", null));
        if (fileRecordId <= 0) return Task.FromResult<(bool, string?, RecordAttachment?)>((false, "File is required.", null));

        var type = entityType.Trim();
        if (type.Length > 64) return Task.FromResult<(bool, string?, RecordAttachment?)>((false, "Entity type is too long.", null));

        return _db.UseAsync(async (context, token) =>
        {
            var file = await context.FileRecords.AsNoTracking()
                .FirstOrDefaultAsync(f => f.FileRecordID == fileRecordId && !f.IsDeleted, token);
            if (file is null) return (false, "File not found.", (RecordAttachment?)null);
            if (file.WebsiteID != websiteId)
                return (false, "File belongs to a different website.", (RecordAttachment?)null);

            var exists = await context.RecordAttachments.AnyAsync(
                a => a.EntityType == type && a.EntityID == entityId && a.FileRecordID == fileRecordId, token);
            if (exists) return (false, "This file is already attached.", (RecordAttachment?)null);

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
            context.RecordAttachments.Add(attachment);
            await context.SaveChangesAsync(token);
            return (true, (string?)null, attachment);
        }, ct);
    }

    public Task<(bool Success, string? Error)> RemoveAsync(long attachmentId, int? memberId, CancellationToken ct = default)
        => _db.UseAsync(async (context, token) =>
        {
            var row = await context.RecordAttachments.FirstOrDefaultAsync(a => a.RecordAttachmentID == attachmentId, token);
            if (row is null) return (false, "Attachment not found.");
            context.RecordAttachments.Remove(row);
            await context.SaveChangesAsync(token);
            return (true, (string?)null);
        }, ct);

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
