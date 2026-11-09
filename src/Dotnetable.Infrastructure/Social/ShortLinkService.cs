using System.Security.Cryptography;
using Dotnetable.Application.Instagram;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Seo;
using Dotnetable.Application.Social;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Social;

public sealed class ShortLinkService : IShortLinkService
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private readonly IDbContextFactory<AppDbContext> _contexts;

    public ShortLinkService(IDbContextFactory<AppDbContext> contexts) => _contexts = contexts;

    public async Task<IReadOnlyList<ShortLinkRow>> ListAsync(int websiteId, CancellationToken ct = default)
    {
        await using var context = await _contexts.CreateDbContextAsync(ct);
        var origin = await OriginAsync(context, websiteId, ct);
        var rows = await context.ShortLinks.AsNoTracking()
            .Where(s => s.WebsiteID == websiteId)
            .OrderByDescending(s => s.ShortLinkID)
            .ToListAsync(ct);
        return rows.Select(row => ToRow(row, origin)).ToList();
    }

    public async Task<string> ToPublicAsync(int websiteId, string targetUrl, CancellationToken ct = default)
    {
        var target = RequireTarget(targetUrl);
        await using var context = await _contexts.CreateDbContextAsync(ct);
        var code = await context.ShortLinks.AsNoTracking()
            .Where(s => s.WebsiteID == websiteId && s.TargetUrl == target)
            .Select(s => s.Code)
            .FirstOrDefaultAsync(ct);
        code ??= await InsertAsync(context, websiteId, target, ct);
        var origin = await OriginAsync(context, websiteId, ct);
        return origin is null ? target : origin + "/l/" + code;
    }

    public async Task<ShortLinkRow> CreateAsync(int websiteId, string targetUrl, CancellationToken ct = default)
    {
        var target = RequireTarget(targetUrl);
        await using var context = await _contexts.CreateDbContextAsync(ct);
        var existing = await context.ShortLinks.AsNoTracking()
            .FirstOrDefaultAsync(s => s.WebsiteID == websiteId && s.TargetUrl == target, ct);
        if (existing is not null)
            return ToRow(existing, await OriginAsync(context, websiteId, ct));

        var code = await InsertAsync(context, websiteId, target, ct);
        var row = await context.ShortLinks.AsNoTracking()
            .FirstAsync(s => s.WebsiteID == websiteId && s.Code == code, ct);
        return ToRow(row, await OriginAsync(context, websiteId, ct));
    }

    public async Task UpdateTargetAsync(int websiteId, int shortLinkId, string targetUrl, CancellationToken ct = default)
    {
        var target = RequireTarget(targetUrl);
        await using var context = await _contexts.CreateDbContextAsync(ct);
        var row = await context.ShortLinks.FirstOrDefaultAsync(s => s.ShortLinkID == shortLinkId && s.WebsiteID == websiteId, ct)
            ?? throw new SocialPublishException("That short link was not found.");
        row.TargetUrl = target;
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int websiteId, int shortLinkId, CancellationToken ct = default)
    {
        await using var context = await _contexts.CreateDbContextAsync(ct);
        await context.ShortLinks
            .Where(s => s.ShortLinkID == shortLinkId && s.WebsiteID == websiteId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<string?> ResolveAsync(int websiteId, string code, CancellationToken ct = default)
    {
        var token = (code ?? "").Trim();
        if (token.Length == 0) return null;
        await using var context = await _contexts.CreateDbContextAsync(ct);
        return await context.ShortLinks.AsNoTracking()
            .Where(s => s.WebsiteID == websiteId && s.Code == token)
            .Select(s => s.TargetUrl)
            .FirstOrDefaultAsync(ct);
    }

    private static async Task<string> InsertAsync(AppDbContext context, int websiteId, string target, CancellationToken ct)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var code = NewCode();
            context.ShortLinks.Add(new ShortLink
            {
                WebsiteID = websiteId,
                Code = code,
                TargetUrl = target,
                CreatedAt = DateTime.UtcNow,
            });
            try
            {
                await context.SaveChangesAsync(ct);
                return code;
            }
            catch (DbUpdateException)
            {
                context.ChangeTracker.Clear();
            }
        }

        throw new SocialPublishException("Could not create a short link.");
    }

    private static async Task<string?> OriginAsync(AppDbContext context, int websiteId, CancellationToken ct)
    {
        var row = await context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .Select(w => new { w.WebsiteAddress, w.SocialLinkBaseUrl })
            .FirstOrDefaultAsync(ct);
        if (row is null) return null;
        try
        {
            var custom = SocialLinkPaths.NormalizeBase(row.SocialLinkBaseUrl);
            if (custom is not null) return custom;
        }
        catch (SocialPublishException)
        {
            return null;
        }

        return IndexNowOrigins.TryParse(row.WebsiteAddress, out var origin, out _) ? origin : null;
    }

    private static ShortLinkRow ToRow(ShortLink row, string? origin) => new()
    {
        Id = row.ShortLinkID,
        Code = row.Code,
        TargetUrl = row.TargetUrl,
        CreatedAt = row.CreatedAt,
        PublicUrl = origin is null ? "/l/" + row.Code : origin + "/l/" + row.Code,
    };

    internal static string RequireTarget(string? targetUrl)
    {
        var target = targetUrl?.Trim() ?? "";
        if (!Uri.TryCreate(target, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new SocialPublishException("The address must be a full http or https link.");
        return target;
    }

    private static string NewCode()
    {
        Span<char> chars = stackalloc char[7];
        for (var i = 0; i < chars.Length; i++)
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        return "LK" + new string(chars);
    }
}
