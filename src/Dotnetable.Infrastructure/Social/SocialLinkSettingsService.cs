using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Social;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Social;

public sealed class SocialLinkSettingsService : ISocialLinkSettings
{
    private readonly IDbContextFactory<AppDbContext> _contexts;

    public SocialLinkSettingsService(IDbContextFactory<AppDbContext> contexts) => _contexts = contexts;

    public async Task<SocialLinkStatus> GetAsync(int websiteId, CancellationToken ct = default)
    {
        await using var context = await _contexts.CreateDbContextAsync(ct);
        var row = await context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .Select(w => new
            {
                w.WebsiteAddress,
                w.SocialLinkBaseUrl,
                w.SocialPostPath,
                w.SocialPagePath,
                w.SocialProductPath,
                w.SocialCategoryPath,
            })
            .FirstOrDefaultAsync(ct);

        return Map(row?.WebsiteAddress, row?.SocialLinkBaseUrl, row?.SocialPostPath, row?.SocialPagePath, row?.SocialProductPath, row?.SocialCategoryPath);
    }

    public async Task SaveAsync(int websiteId, SocialLinkInput input, CancellationToken ct = default)
    {
        var baseUrl = SocialLinkPaths.NormalizeBase(input.BaseUrl);
        var post = SocialLinkPaths.NormalizePath(input.PostPath);
        var page = SocialLinkPaths.NormalizePath(input.PagePath);
        var product = SocialLinkPaths.NormalizePath(input.ProductPath);
        var category = SocialLinkPaths.NormalizePath(input.CategoryPath);
        SocialLinkPaths.EnsureDistinct(
            SocialLinkPaths.Effective(post, SocialLinkPaths.DefaultPost),
            SocialLinkPaths.Effective(page, SocialLinkPaths.DefaultPage),
            SocialLinkPaths.Effective(product, SocialLinkPaths.DefaultProduct),
            SocialLinkPaths.Effective(category, SocialLinkPaths.DefaultCategory));

        await using var context = await _contexts.CreateDbContextAsync(ct);
        var row = await context.Websites.FirstOrDefaultAsync(w => w.WebsiteID == websiteId, ct)
            ?? throw new Application.Instagram.SocialPublishException("Website was not found.");
        row.SocialLinkBaseUrl = baseUrl;
        row.SocialPostPath = post;
        row.SocialPagePath = page;
        row.SocialProductPath = product;
        row.SocialCategoryPath = category;
        await context.SaveChangesAsync(ct);
    }

    internal static SocialLinkStatus Map(
        string? websiteAddress, string? baseUrl, string? post, string? page, string? product, string? category) =>
        new()
        {
            WebsiteAddress = websiteAddress,
            BaseUrl = baseUrl,
            PostPath = post,
            PagePath = page,
            ProductPath = product,
            CategoryPath = category,
            Effective = new SocialLinkMap
            {
                PostPath = SocialLinkPaths.Effective(post, SocialLinkPaths.DefaultPost),
                PagePath = SocialLinkPaths.Effective(page, SocialLinkPaths.DefaultPage),
                ProductPath = SocialLinkPaths.Effective(product, SocialLinkPaths.DefaultProduct),
                CategoryPath = SocialLinkPaths.Effective(category, SocialLinkPaths.DefaultCategory),
            },
        };
}
