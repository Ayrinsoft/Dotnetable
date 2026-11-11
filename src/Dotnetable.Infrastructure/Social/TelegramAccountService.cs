using Dotnetable.Application.DTOs;
using Dotnetable.Application.Social;
using Dotnetable.Application.Instagram;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Social;

public sealed class TelegramAccountService : ITelegramAccountService
{
    private readonly IDbContextFactory<AppDbContext> _contexts;
    private readonly ITelegramBot _bot;

    public TelegramAccountService(IDbContextFactory<AppDbContext> contexts, ITelegramBot bot)
    {
        _contexts = contexts;
        _bot = bot;
    }

    public async Task<TelegramAccountStatus> GetAsync(int websiteId, CancellationToken ct = default)
    {
        var rows = await ListAsync(websiteId, ct);
        return rows.FirstOrDefault(row => row.Connected) ?? new TelegramAccountStatus();
    }

    public async Task<IReadOnlyList<TelegramAccountStatus>> ListAsync(int websiteId, CancellationToken ct = default)
    {
        await using var context = await _contexts.CreateDbContextAsync(ct);
        var rows = await context.WebsiteTelegramAccounts.AsNoTracking()
            .Where(a => a.WebsiteID == websiteId)
            .OrderBy(a => a.WebsiteTelegramAccountID)
            .ToListAsync(ct);
        return rows.Select(Status).ToList();
    }

    public async Task<TelegramAccountStatus> SaveAsync(int websiteId, TelegramAccountInput input, CancellationToken ct = default)
    {
        var chatId = TelegramChatId.Normalize(input.ChatId);
        var (title, language) = SocialChannelChoice.Read(input.Title, input.LanguageCode);

        await using var context = await _contexts.CreateDbContextAsync(ct);
        var row = await FindAsync(context, websiteId, input.AccountId, ct);
        var token = string.IsNullOrWhiteSpace(input.BotToken) ? row?.BotToken : input.BotToken.Trim();
        if (string.IsNullOrWhiteSpace(token))
            throw new SocialPublishException("Enter the bot token from BotFather.");

        using var proxy = SocialProxyLease.Open(await SocialProxyLease.ReadAsync(context, websiteId, ct));
        await _bot.EnsureChatAsync(token, chatId, ct);
        var username = await _bot.GetUsernameAsync(token, ct);
        if (row is null)
        {
            row = new WebsiteTelegramAccount { WebsiteID = websiteId };
            context.WebsiteTelegramAccounts.Add(row);
        }

        row.Title = title;
        row.LanguageCode = language;
        row.ChatId = chatId;
        row.BotToken = token;
        row.Username = string.IsNullOrWhiteSpace(username) ? row.Username : username.Trim().TrimStart('@');
        row.IsActive = true;
        row.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return Status(row);
    }

    public async Task DisconnectAsync(int websiteId, int accountId, CancellationToken ct = default)
    {
        await using var context = await _contexts.CreateDbContextAsync(ct);
        await context.WebsiteTelegramAccounts
            .Where(a => a.WebsiteID == websiteId && a.WebsiteTelegramAccountID == accountId)
            .ExecuteDeleteAsync(ct);
    }

    private static async Task<WebsiteTelegramAccount?> FindAsync(AppDbContext context, int websiteId, int accountId, CancellationToken ct)
    {
        if (accountId <= 0) return null;
        return await context.WebsiteTelegramAccounts
            .FirstOrDefaultAsync(a => a.WebsiteTelegramAccountID == accountId && a.WebsiteID == websiteId, ct)
            ?? throw new SocialPublishException("That Telegram channel was not found.");
    }

    private static TelegramAccountStatus Status(WebsiteTelegramAccount row) => new()
    {
        AccountId = row.WebsiteTelegramAccountID,
        Title = row.Title,
        LanguageCode = row.LanguageCode,
        Connected = row.IsActive && !string.IsNullOrWhiteSpace(row.ChatId) && !string.IsNullOrWhiteSpace(row.BotToken),
        ChatId = row.ChatId,
        Username = row.Username,
        HasToken = !string.IsNullOrWhiteSpace(row.BotToken),
    };
}
