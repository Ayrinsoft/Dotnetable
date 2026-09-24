using System.Net;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Messaging;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Backs Messages → Send message: one message, any channel, to hand-picked members/customers, typed-in
/// addresses, or everyone. Delivery goes through the ordinary senders, so it uses the website's own
/// gateways/accounts (with their usual fallbacks) and is recorded in the message log by them; this
/// class only sets the log scope (who sent it, to whom) and writes the log row for in-app messages,
/// which have no sender of their own.
/// </summary>
public sealed class MessageComposerService : IMessageComposerService
{
    private const int MaxErrors = 10;

    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IEmailService _email;
    private readonly ISmsSender _sms;
    private readonly IWhatsAppSender _whatsApp;
    private readonly IMessageLogService _log;
    private readonly ILogger<MessageComposerService> _logger;

    public MessageComposerService(
        IDbContextFactory<AppDbContext> contextFactory,
        IEmailService email,
        ISmsSender sms,
        IWhatsAppSender whatsApp,
        IMessageLogService log,
        ILogger<MessageComposerService> logger)
    {
        _contextFactory = contextFactory;
        _email = email;
        _sms = sms;
        _whatsApp = whatsApp;
        _log = log;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MessageRecipient>> SearchRecipientsAsync(int websiteId, string? term, int take = 20,
        CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        var t = term?.Trim();
        take = Math.Clamp(take, 1, 50);

        var members = _context.Members.AsNoTracking().Where(m => m.WebsiteID == websiteId && m.Active);
        var clients = _context.WebsiteClients.AsNoTracking().Where(c => c.WebsiteID == websiteId && c.Active);

        if (!string.IsNullOrEmpty(t))
        {
            members = members.Where(m => m.Givenname.Contains(t) || m.Surname.Contains(t) || m.Username.Contains(t)
                                         || m.Email.Contains(t) || m.CellphoneNumber.Contains(t));
            clients = clients.Where(c => (c.Givenname != null && c.Givenname.Contains(t))
                                         || (c.Surname != null && c.Surname.Contains(t))
                                         || (c.Email != null && c.Email.Contains(t))
                                         || (c.Cellphone != null && c.Cellphone.Contains(t)));
        }

        var memberRows = await members.OrderBy(m => m.Givenname).Take(take).Select(MemberProjection).ToListAsync(ct);
        var clientRows = await clients.OrderByDescending(c => c.WebsiteClientID).Take(take).Select(ClientProjection).ToListAsync(ct);

        return memberRows.Select(ToRecipient).Concat(clientRows.Select(ToRecipient)).Take(take).ToList();
    }

    public async Task<int> CountAudienceAsync(int websiteId, MessageAudience audience, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        return audience switch
        {
            MessageAudience.AllMembers => await _context.Members.CountAsync(m => m.WebsiteID == websiteId && m.Active, ct),
            MessageAudience.AllClients => await _context.WebsiteClients.CountAsync(c => c.WebsiteID == websiteId && c.Active, ct),
            _ => 0,
        };
    }

    public async Task<IReadOnlyDictionary<MessageChannel, bool>> GetChannelAvailabilityAsync(int websiteId, CancellationToken ct = default) =>
        new Dictionary<MessageChannel, bool>
        {
            [MessageChannel.Email] = await _email.IsConfiguredAsync(websiteId, ct),
            [MessageChannel.Sms] = await _sms.IsConfiguredAsync(websiteId, ct),
            [MessageChannel.WhatsApp] = await _whatsApp.IsConfiguredAsync(websiteId, ct),
            [MessageChannel.InApp] = true,
        };

    public async Task<ComposeMessageResult> SendAsync(ComposeMessageRequest request, IProgress<ComposeProgress>? progress = null,
        CancellationToken ct = default)
    {
        var result = new ComposeMessageResult();
        if (string.IsNullOrWhiteSpace(request.Body))
            throw new InvalidOperationException("The message is empty.");

        var recipients = await ResolveRecipientsAsync(request, ct);
        var total = recipients.Count;
        var done = 0;
        progress?.Report(new ComposeProgress(0, total));

        using var batchScope = MessageLogScope.Begin(MessageLogSources.Manual,
            sentByMemberId: request.SentByMemberID, sentByName: request.SentByName);

        foreach (var r in recipients)
        {
            ct.ThrowIfCancellationRequested();

            if (!HasAddress(r, request.Channel))
            {
                result.Skipped++;
            }
            else
            {
                using var recipientScope = MessageLogScope.Begin(recipientType: r.Type, recipientId: r.Id,
                    recipientName: r.Id is null ? null : r.DisplayName);
                var (ok, error) = await DeliverAsync(request, r, ct);
                if (ok) result.Sent++;
                else
                {
                    result.Failed++;
                    if (error is not null && result.Errors.Count < MaxErrors && !result.Errors.Contains(error))
                        result.Errors.Add(error);
                }
            }

            progress?.Report(new ComposeProgress(++done, total));
        }

        return result;
    }

    private async Task<(bool Ok, string? Error)> DeliverAsync(ComposeMessageRequest request, MessageRecipient r, CancellationToken ct)
    {
        var name = string.IsNullOrWhiteSpace(r.DisplayName) || r.Id is null ? "" : r.DisplayName;
        var body = Personalize(request.Body, name);
        var subject = Personalize(request.Subject ?? "", name).Trim();

        try
        {
            switch (request.Channel)
            {
                case MessageChannel.Email:
                    var html = request.BodyIsHtml ? body : PlainToHtml(body);
                    await _email.SendAsync(request.WebsiteID, request.EmailAccountType, r.Email!,
                        string.IsNullOrEmpty(subject) ? "(no subject)" : subject, html, ct);
                    return (true, null);

                case MessageChannel.Sms:
                    return await _sms.SendAsync(request.WebsiteID, r.CountryCode ?? "", r.Cellphone!, body, ct)
                        ? (true, null)
                        : (false, "SMS gateway did not accept the message — see the message log for details.");

                case MessageChannel.WhatsApp:
                    return await _whatsApp.SendAsync(request.WebsiteID, r.CountryCode ?? "", r.Cellphone!, body, ct)
                        ? (true, null)
                        : (false, "WhatsApp gateway did not accept the message — see the message log for details.");

                case MessageChannel.InApp:
                    await SendInAppAsync(request, r, subject, body, ct);
                    return (true, null);

                default:
                    return (false, $"Unsupported channel {request.Channel}.");
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Manual {Channel} message to {Recipient} failed.", request.Channel, r.DisplayName);
            return (false, ex.Message);
        }
    }

    /// <summary>In-app messages have no sender service, so this writes the notification and its log row.</summary>
    private async Task SendInAppAsync(ComposeMessageRequest request, MessageRecipient r, string subject, string body, CancellationToken ct)
    {
        var title = Cut(string.IsNullOrEmpty(subject) ? (request.SentByName ?? "Message") : subject, 200);
        var message = Cut(body, 1000);
        var url = string.IsNullOrWhiteSpace(request.ActionUrl) ? null : Cut(request.ActionUrl.Trim(), 256);

        await using (var _context = await _contextFactory.CreateDbContextAsync(ct))
        {
            _context.AdminNotifications.Add(new AdminNotification
            {
                MemberID = r.Id!.Value,
                WebsiteID = request.WebsiteID,
                NotificationType = (byte)AdminNotificationType.DirectMessage,
                Title = title,
                Message = message,
                ActionUrl = url,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
            });
            await _context.SaveChangesAsync(ct);
        }

        await _log.WriteAsync(new MessageLogEntry
        {
            WebsiteID = request.WebsiteID,
            Channel = MessageChannel.InApp,
            Success = true,
            Recipient = r.Email ?? r.DisplayName,
            Subject = title,
            Body = message,
            Provider = "Admin inbox",
        }, ct);
    }

    /// <summary>
    /// Hand-picked members/customers are re-read from the database scoped to the website, so a forged
    /// id from another website cannot be addressed; then the audience is added and duplicates dropped.
    /// </summary>
    private async Task<List<MessageRecipient>> ResolveRecipientsAsync(ComposeMessageRequest request, CancellationToken ct)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        var websiteId = request.WebsiteID;
        var list = new List<MessageRecipient>();

        var memberIds = request.Recipients.Where(r => r.Type == MessageRecipientType.Member && r.Id is not null)
            .Select(r => r.Id!.Value).Distinct().ToList();
        var clientIds = request.Recipients.Where(r => r.Type == MessageRecipientType.Client && r.Id is not null)
            .Select(r => r.Id!.Value).Distinct().ToList();

        var includeAllMembers = request.Audience == MessageAudience.AllMembers;
        var includeAllClients = request.Audience == MessageAudience.AllClients;

        if (includeAllMembers || memberIds.Count > 0)
        {
            var rows = await _context.Members.AsNoTracking()
                .Where(m => m.WebsiteID == websiteId && m.Active && (includeAllMembers || memberIds.Contains(m.MemberID)))
                .Select(MemberProjection).ToListAsync(ct);
            list.AddRange(rows.Select(ToRecipient));
        }

        if (includeAllClients || clientIds.Count > 0)
        {
            var rows = await _context.WebsiteClients.AsNoTracking()
                .Where(c => c.WebsiteID == websiteId && c.Active && (includeAllClients || clientIds.Contains(c.WebsiteClientID)))
                .Select(ClientProjection).ToListAsync(ct);
            list.AddRange(rows.Select(ToRecipient));
        }

        // Typed-in addresses. In-app messages need a member account, so they are skipped there.
        list.AddRange(request.Recipients.Where(r => r.Id is null));

        // One message per address: the same person picked twice, or picked and also in the audience.
        return list
            .GroupBy(r => AddressKey(r, request.Channel))
            .Select(g => g.OrderByDescending(r => r.Id is not null).First())
            .ToList();
    }

    private static string AddressKey(MessageRecipient r, MessageChannel channel) => channel switch
    {
        MessageChannel.Email => string.IsNullOrWhiteSpace(r.Email) ? "none:" + r.Key : "e:" + r.Email.Trim().ToLowerInvariant(),
        MessageChannel.Sms or MessageChannel.WhatsApp => string.IsNullOrWhiteSpace(r.Cellphone)
            ? "none:" + r.Key
            : "p:" + new string(((r.CountryCode ?? "") + r.Cellphone.TrimStart('0')).Where(char.IsDigit).ToArray()),
        _ => r.Key,
    };

    private static bool HasAddress(MessageRecipient r, MessageChannel channel) => channel switch
    {
        MessageChannel.Email => !string.IsNullOrWhiteSpace(r.Email) && r.Email.Contains('@'),
        MessageChannel.Sms or MessageChannel.WhatsApp => !string.IsNullOrWhiteSpace(r.Cellphone),
        MessageChannel.InApp => r.Type == MessageRecipientType.Member && r.Id is not null,
        _ => false,
    };

    private static string Personalize(string text, string name) =>
        text.Replace("{name}", name, StringComparison.OrdinalIgnoreCase).Replace("{نام}", name, StringComparison.Ordinal);

    private static string PlainToHtml(string text) =>
        "<div dir=\"auto\" style=\"font-family:Tahoma,Arial,sans-serif;font-size:14px;line-height:1.8\">" +
        WebUtility.HtmlEncode(text).Replace("\r\n", "\n").Replace("\n", "<br>") +
        "</div>";

    private static string Cut(string value, int max) => value.Length <= max ? value : value[..max];

    private sealed record PersonRow(MessageRecipientType Type, int Id, string? Givenname, string? Surname, string? Fallback,
        string? Email, string? CountryCode, string? Cellphone);

    private static readonly System.Linq.Expressions.Expression<Func<Member, PersonRow>> MemberProjection =
        m => new PersonRow(MessageRecipientType.Member, m.MemberID, m.Givenname, m.Surname, m.Username, m.Email, m.CountryCode, m.CellphoneNumber);

    private static readonly System.Linq.Expressions.Expression<Func<WebsiteClient, PersonRow>> ClientProjection =
        c => new PersonRow(MessageRecipientType.Client, c.WebsiteClientID, c.Givenname, c.Surname, null, c.Email, c.CountryCode, c.Cellphone);

    private static MessageRecipient ToRecipient(PersonRow p)
    {
        var name = string.Join(' ', new[] { p.Givenname, p.Surname }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
        if (name.Length == 0) name = p.Fallback ?? p.Email ?? p.Cellphone ?? $"#{p.Id}";
        return new MessageRecipient
        {
            Type = p.Type,
            Id = p.Id,
            DisplayName = name,
            Email = string.IsNullOrWhiteSpace(p.Email) ? null : p.Email,
            CountryCode = p.CountryCode,
            Cellphone = string.IsNullOrWhiteSpace(p.Cellphone) ? null : p.Cellphone,
        };
    }
}
