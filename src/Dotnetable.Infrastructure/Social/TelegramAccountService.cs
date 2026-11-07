using Dotnetable.Application.DTOs;
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
        await using var context = await _contexts.CreateDbContextAsync(ct);
        var row = await context.WebsiteTelegramAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.WebsiteID == websiteId, ct);
        return Status(row);
    }

    public async Task<TelegramAccountStatus> SaveAsync(int websiteId, TelegramAccountInput input, CancellationToken ct = default)
    {
        var chatId = (input.ChatId ?? "").Trim();
        if (chatId.Length == 0)
            throw new SocialPublishException("Enter the Telegram chat id, such as @channel.");

        await using var context = await _contexts.CreateDbContextAsync(ct);
        var row = await context.WebsiteTelegramAccounts.FirstOrDefaultAsync(a => a.WebsiteID == websiteId, ct);
        var token = string.IsNullOrWhiteSpace(input.BotToken) ? row?.BotToken : input.BotToken.Trim();
        if (string.IsNullOrWhiteSpace(token))
            throw new SocialPublishException("Enter the bot token from BotFather.");

        using var proxy = await SocialProxyLease.OpenAsync(context, websiteId, ct);
        var username = await _bot.GetUsernameAsync(token, ct);
        await _bot.EnsureChatAsync(token, chatId, ct);
        if (row is null)
        {
            row = new WebsiteTelegramAccount { WebsiteID = websiteId };
            context.WebsiteTelegramAccounts.Add(row);
        }

        row.ChatId = chatId;
        row.BotToken = token;
        row.Username = string.IsNullOrWhiteSpace(username) ? row.Username : username.Trim().TrimStart('@');
        row.IsActive = true;
        row.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return Status(row);
    }

    public async Task DisconnectAsync(int websiteId, CancellationToken ct = default)
    {
        await using var context = await _contexts.CreateDbContextAsync(ct);
        await context.WebsiteTelegramAccounts.Where(a => a.WebsiteID == websiteId).ExecuteDeleteAsync(ct);
    }

    private static TelegramAccountStatus Status(WebsiteTelegramAccount? row) => new()
    {
        Connected = row is { IsActive: true } && !string.IsNullOrWhiteSpace(row.ChatId) && !string.IsNullOrWhiteSpace(row.BotToken),
        ChatId = row?.ChatId,
        Username = row?.Username,
        HasToken = !string.IsNullOrWhiteSpace(row?.BotToken),
    };
}
