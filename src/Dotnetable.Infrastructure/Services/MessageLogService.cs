using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Messaging;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// The outgoing-message log. Rows are written by the senders on their own short-lived context — never
/// the caller's — so a send inside someone else's transaction is still recorded if that transaction
/// rolls back, and a failed log write never reaches the caller.
/// </summary>
public sealed class MessageLogService : IMessageLogService
{
    /// <summary>Bodies beyond this are cut; a marketing email can be large and the log is not an archive.</summary>
    private const int MaxBodyLength = 20_000;

    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<MessageLogService> _logger;

    public MessageLogService(IDbContextFactory<AppDbContext> contextFactory, ILogger<MessageLogService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task WriteAsync(MessageLogEntry entry, CancellationToken ct = default)
    {
        var scope = MessageLogScope.Current;
        var redact = scope?.RedactBody ?? false;

        try
        {
            await using var _context = await _contextFactory.CreateDbContextAsync(CancellationToken.None);
            _context.MessageLogs.Add(new MessageLog
            {
                WebsiteID = entry.WebsiteID,
                Channel = (byte)entry.Channel,
                Status = (byte)(entry.Success ? MessageLogStatus.Sent : MessageLogStatus.Failed),
                Recipient = Cut(entry.Recipient, 256) ?? "",
                RecipientName = Cut(scope?.RecipientName, 200),
                RecipientType = (byte)(scope?.RecipientType ?? MessageRecipientType.Other),
                RecipientID = scope?.RecipientID,
                Subject = Cut(entry.Subject, 300),
                Body = redact ? null : Cut(entry.Body, MaxBodyLength),
                IsBodyRedacted = redact,
                Provider = Cut(entry.Provider, 150),
                Source = Cut(scope?.Source, 32) ?? MessageLogSources.System,
                Error = Cut(entry.Error, 1000),
                SentByMemberID = scope?.SentByMemberID,
                SentByName = Cut(scope?.SentByName, 200),
                CreatedAt = DateTime.UtcNow,
            });
            // Not the caller's token: a request aborted right after the gateway accepted the message
            // must still leave a record that the message went out.
            await _context.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not record {Channel} message for website {WebsiteId} in the message log.",
                entry.Channel, entry.WebsiteID);
        }
    }

    public async Task<PagedResult<MessageLogListItem>> GetPagedAsync(MessageLogFilter filter, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = Filter(_context.MessageLogs.AsNoTracking(), filter);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(MessageLog.CreatedAt), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .Select(m => new MessageLogListItem
            {
                MessageLogID = m.MessageLogID,
                WebsiteID = m.WebsiteID,
                WebsiteName = m.Website.BrandName ?? m.Website.TradeName,
                Channel = (MessageChannel)m.Channel,
                Status = (MessageLogStatus)m.Status,
                Recipient = m.Recipient,
                RecipientName = m.RecipientName,
                RecipientType = (MessageRecipientType)m.RecipientType,
                RecipientID = m.RecipientID,
                Subject = m.Subject,
                IsBodyRedacted = m.IsBodyRedacted,
                Provider = m.Provider,
                Source = m.Source,
                Error = m.Error,
                SentByName = m.SentByName,
                CreatedAt = m.CreatedAt,
            })
            .ToListAsync(ct);

        return new PagedResult<MessageLogListItem> { Items = items, TotalCount = total };
    }

    public async Task<MessageLogSummary> GetSummaryAsync(MessageLogFilter filter, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var groups = await Filter(_context.MessageLogs.AsNoTracking(), filter)
            .GroupBy(m => new { m.Channel, m.Status })
            .Select(g => new { g.Key.Channel, g.Key.Status, Count = g.Count() })
            .ToListAsync(ct);

        return new MessageLogSummary
        {
            Total = groups.Sum(g => g.Count),
            Sent = groups.Where(g => g.Status == (byte)MessageLogStatus.Sent).Sum(g => g.Count),
            Failed = groups.Where(g => g.Status == (byte)MessageLogStatus.Failed).Sum(g => g.Count),
            ByChannel = groups.GroupBy(g => (MessageChannel)g.Channel).ToDictionary(g => g.Key, g => g.Sum(x => x.Count)),
        };
    }

    public async Task<MessageLog?> GetByIdAsync(long id, int? websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        return await _context.MessageLogs.AsNoTracking()
            .FirstOrDefaultAsync(m => m.MessageLogID == id && (websiteId == null || m.WebsiteID == websiteId), ct);
    }

    public async Task<int> PurgeAsync(int? websiteId, DateTime beforeUtc, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        return await _context.MessageLogs
            .Where(m => m.CreatedAt < beforeUtc && (websiteId == null || m.WebsiteID == websiteId))
            .ExecuteDeleteAsync(ct);
    }

    private static IQueryable<MessageLog> Filter(IQueryable<MessageLog> q, MessageLogFilter f)
    {
        if (f.WebsiteID is int websiteId)
            q = q.Where(m => m.WebsiteID == websiteId);
        if (f.Channel is MessageChannel channel)
            q = q.Where(m => m.Channel == (byte)channel);
        if (f.Status is MessageLogStatus status)
            q = q.Where(m => m.Status == (byte)status);
        if (!string.IsNullOrWhiteSpace(f.Source))
            q = q.Where(m => m.Source == f.Source);
        if (!string.IsNullOrWhiteSpace(f.Recipient))
        {
            var term = f.Recipient.Trim();
            q = q.Where(m => m.Recipient.Contains(term) || (m.RecipientName != null && m.RecipientName.Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(f.Text))
        {
            var term = f.Text.Trim();
            q = q.Where(m => (m.Subject != null && m.Subject.Contains(term)) || (m.Body != null && m.Body.Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(f.SentBy))
        {
            var term = f.SentBy.Trim();
            q = q.Where(m => m.SentByName != null && m.SentByName.Contains(term));
        }
        if (f.FromUtc is DateTime from)
            q = q.Where(m => m.CreatedAt >= from);
        if (f.ToUtc is DateTime to)
            q = q.Where(m => m.CreatedAt < to);
        return q;
    }

    private static string? Cut(string? value, int max) =>
        value is null ? null : (value.Length <= max ? value : value[..max]);
}
