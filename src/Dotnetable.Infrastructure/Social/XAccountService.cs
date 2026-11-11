using Dotnetable.Application.DTOs;
using Dotnetable.Application.Social;
using Dotnetable.Application.Instagram;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Social;

public sealed class XAccountService : IXAccountService
{
    private readonly IDbContextFactory<AppDbContext> _contexts;
    private readonly IXApi _api;

    public XAccountService(IDbContextFactory<AppDbContext> contexts, IXApi api)
    {
        _contexts = contexts;
        _api = api;
    }

    public async Task<XAccountStatus> GetAsync(int websiteId, CancellationToken ct = default)
    {
        var rows = await ListAsync(websiteId, ct);
        return rows.FirstOrDefault(row => row.Connected) ?? new XAccountStatus();
    }

    public async Task<IReadOnlyList<XAccountStatus>> ListAsync(int websiteId, CancellationToken ct = default)
    {
        await using var context = await _contexts.CreateDbContextAsync(ct);
        var rows = await context.WebsiteXAccounts.AsNoTracking()
            .Where(a => a.WebsiteID == websiteId)
            .OrderBy(a => a.WebsiteXAccountID)
            .ToListAsync(ct);
        return rows.Select(Status).ToList();
    }

    public async Task<XAccountStatus> SaveAsync(int websiteId, XAccountInput input, CancellationToken ct = default)
    {
        var (title, language) = SocialChannelChoice.Read(input.Title, input.LanguageCode);
        await using var context = await _contexts.CreateDbContextAsync(ct);
        var row = await FindAsync(context, websiteId, input.AccountId, ct);
        var apiKey = Keep(input.ApiKey, row?.ApiKey);
        var apiSecret = Keep(input.ApiSecret, row?.ApiSecret);
        var accessToken = Keep(input.AccessToken, row?.AccessToken);
        var accessSecret = Keep(input.AccessTokenSecret, row?.AccessTokenSecret);
        if (apiKey is null || apiSecret is null || accessToken is null || accessSecret is null)
            throw new SocialPublishException("Enter the API key, API secret, access token, and access token secret.");

        using var proxy = SocialProxyLease.Open(await SocialProxyLease.ReadAsync(context, websiteId, ct));
        var username = await _api.GetUsernameAsync(new XCredentials(apiKey, apiSecret, accessToken, accessSecret), ct);
        if (row is null)
        {
            row = new WebsiteXAccount { WebsiteID = websiteId };
            context.WebsiteXAccounts.Add(row);
        }

        row.Title = title;
        row.LanguageCode = language;
        row.ApiKey = apiKey;
        row.ApiSecret = apiSecret;
        row.AccessToken = accessToken;
        row.AccessTokenSecret = accessSecret;
        row.Username = string.IsNullOrWhiteSpace(username) ? row.Username : username.Trim().TrimStart('@');
        row.IsActive = true;
        row.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return Status(row);
    }

    public async Task DisconnectAsync(int websiteId, int accountId, CancellationToken ct = default)
    {
        await using var context = await _contexts.CreateDbContextAsync(ct);
        await context.WebsiteXAccounts
            .Where(a => a.WebsiteID == websiteId && a.WebsiteXAccountID == accountId)
            .ExecuteDeleteAsync(ct);
    }

    private static async Task<WebsiteXAccount?> FindAsync(AppDbContext context, int websiteId, int accountId, CancellationToken ct)
    {
        if (accountId <= 0) return null;
        return await context.WebsiteXAccounts
            .FirstOrDefaultAsync(a => a.WebsiteXAccountID == accountId && a.WebsiteID == websiteId, ct)
            ?? throw new SocialPublishException("That X channel was not found.");
    }

    private static string? Keep(string? incoming, string? saved) =>
        string.IsNullOrWhiteSpace(incoming) ? (string.IsNullOrWhiteSpace(saved) ? null : saved) : incoming.Trim();

    private static XAccountStatus Status(WebsiteXAccount row) => new()
    {
        AccountId = row.WebsiteXAccountID,
        Title = row.Title,
        LanguageCode = row.LanguageCode,
        Connected = row.IsActive
            && !string.IsNullOrWhiteSpace(row.ApiKey)
            && !string.IsNullOrWhiteSpace(row.ApiSecret)
            && !string.IsNullOrWhiteSpace(row.AccessToken)
            && !string.IsNullOrWhiteSpace(row.AccessTokenSecret),
        Username = row.Username,
        HasKeys = !string.IsNullOrWhiteSpace(row.ApiKey)
            && !string.IsNullOrWhiteSpace(row.ApiSecret)
            && !string.IsNullOrWhiteSpace(row.AccessToken)
            && !string.IsNullOrWhiteSpace(row.AccessTokenSecret),
    };
}
