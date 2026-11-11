using Dotnetable.Application.DTOs;
using Dotnetable.Application.Instagram;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Application.Social;
using Dotnetable.Infrastructure.Social;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Instagram;

public sealed class InstagramAccountService : IInstagramAccountService
{
    private readonly IDbContextFactory<AppDbContext> _contexts;
    private readonly IInstagramGraph _graph;

    public InstagramAccountService(IDbContextFactory<AppDbContext> contexts, IInstagramGraph graph)
    {
        _contexts = contexts;
        _graph = graph;
    }

    public async Task<InstagramAccountStatus> GetAsync(int websiteId, CancellationToken ct = default)
    {
        var rows = await ListAsync(websiteId, ct);
        return rows.FirstOrDefault(row => row.Connected) ?? new InstagramAccountStatus();
    }

    public async Task<IReadOnlyList<InstagramAccountStatus>> ListAsync(int websiteId, CancellationToken ct = default)
    {
        await using var context = await _contexts.CreateDbContextAsync(ct);
        var rows = await context.WebsiteInstagramAccounts.AsNoTracking()
            .Where(a => a.WebsiteID == websiteId)
            .OrderBy(a => a.WebsiteInstagramAccountID)
            .ToListAsync(ct);
        return rows.Select(Status).ToList();
    }

    public async Task<InstagramAccountStatus> SaveAsync(int websiteId, InstagramAccountInput input, CancellationToken ct = default)
    {
        var igUserId = (input.IgUserId ?? "").Trim();
        if (igUserId.Length == 0)
            throw new InstagramException("Enter the Instagram account id.");
        var (title, language) = SocialChannelChoice.Read(input.Title, input.LanguageCode);

        await using var context = await _contexts.CreateDbContextAsync(ct);
        var row = await FindAsync(context, websiteId, input.AccountId, ct);
        var token = string.IsNullOrWhiteSpace(input.AccessToken) ? row?.AccessToken : input.AccessToken.Trim();
        if (string.IsNullOrWhiteSpace(token))
            throw new InstagramException("Enter the Page access token.");

        using var proxy = SocialProxyLease.Open(await SocialProxyLease.ReadAsync(context, websiteId, ct));
        var username = await _graph.GetUsernameAsync(igUserId, token, ct);
        if (row is null)
        {
            row = new WebsiteInstagramAccount { WebsiteID = websiteId };
            context.WebsiteInstagramAccounts.Add(row);
        }

        row.Title = title;
        row.LanguageCode = language;
        row.IgUserId = igUserId;
        row.AccessToken = token;
        row.PageId = string.IsNullOrWhiteSpace(input.PageId) ? null : input.PageId.Trim();
        row.Username = string.IsNullOrWhiteSpace(username) ? row.Username : username.Trim().TrimStart('@');
        row.IsActive = true;
        row.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return Status(row);
    }

    public async Task DisconnectAsync(int websiteId, int accountId, CancellationToken ct = default)
    {
        await using var context = await _contexts.CreateDbContextAsync(ct);
        await context.WebsiteInstagramAccounts
            .Where(a => a.WebsiteID == websiteId && a.WebsiteInstagramAccountID == accountId)
            .ExecuteDeleteAsync(ct);
    }

    private static async Task<WebsiteInstagramAccount?> FindAsync(AppDbContext context, int websiteId, int accountId, CancellationToken ct)
    {
        if (accountId <= 0) return null;
        return await context.WebsiteInstagramAccounts
            .FirstOrDefaultAsync(a => a.WebsiteInstagramAccountID == accountId && a.WebsiteID == websiteId, ct)
            ?? throw new SocialPublishException("That Instagram channel was not found.");
    }

    private static InstagramAccountStatus Status(WebsiteInstagramAccount row) => new()
    {
        AccountId = row.WebsiteInstagramAccountID,
        Title = row.Title,
        LanguageCode = row.LanguageCode,
        Connected = row.IsActive && !string.IsNullOrWhiteSpace(row.IgUserId) && !string.IsNullOrWhiteSpace(row.AccessToken),
        IgUserId = row.IgUserId,
        Username = row.Username,
        PageId = row.PageId,
        HasToken = !string.IsNullOrWhiteSpace(row.AccessToken),
    };
}
