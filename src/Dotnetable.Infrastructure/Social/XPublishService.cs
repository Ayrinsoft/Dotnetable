using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Instagram;
using Dotnetable.Application.Social;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Instagram;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Social;

public sealed class XPublishService
{
    private readonly IDbContextFactory<AppDbContext> _contexts;
    private readonly IInstagramMediaHost _host;
    private readonly IXApi _api;
    private readonly IShortLinkService? _shortLinks;

    public XPublishService(
        IDbContextFactory<AppDbContext> contexts,
        IInstagramMediaHost host,
        IXApi api,
        IShortLinkService? shortLinks = null)
    {
        _contexts = contexts;
        _host = host;
        _api = api;
        _shortLinks = shortLinks;
    }

    public Task PublishAsync(int postId, InstagramPublishRequest request, CancellationToken ct = default) =>
        PublishOwnerAsync(postId, product: false, request, ct);

    public Task PublishProductAsync(int productId, InstagramPublishRequest request, CancellationToken ct = default) =>
        PublishOwnerAsync(productId, product: true, request, ct);

    private async Task PublishOwnerAsync(int ownerId, bool product, InstagramPublishRequest request, CancellationToken ct)
    {
        await using var context = await _contexts.CreateDbContextAsync(ct);
        int websiteId;
        string excerpt;
        string? slug;
        if (product)
        {
            var row = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductID == ownerId, ct)
                ?? throw new SocialPublishException("Product was not found.");
            websiteId = row.WebsiteID;
            excerpt = await SocialContentText.ExcerptAsync(context, product: true, ownerId, row.ShortDescription ?? "", request.ContentLanguageCode, ct);
            slug = row.Slug;
        }
        else
        {
            var post = await context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.PostID == ownerId, ct)
                ?? throw new SocialPublishException("Post was not found.");
            websiteId = post.WebsiteID;
            excerpt = await SocialContentText.ExcerptAsync(context, product: false, ownerId, post.Excerpt ?? "", request.ContentLanguageCode, ct);
            slug = post.Slug;
        }

        using var proxy = SocialProxyLease.Open(await SocialProxyLease.ReadAsync(context, websiteId, ct));
        var account = await SocialAccountLookup.XAsync(context, websiteId, request.XAccountId, ct);
        var site = await context.Websites.AsNoTracking().Where(w => w.WebsiteID == websiteId).Select(w => w.WebsiteAddress).FirstOrDefaultAsync(ct);
        string? link = null;
        string? tags = null;
        var source = request.UsePrimaryExcerpt ? excerpt : request.Caption ?? "";
        if (!request.SkipAppend)
        {
            link = await SocialLinks.ForContentAsync(context, websiteId, product, slug, ct);
            link = await SocialLinks.MaybeShortAsync(_shortLinks, websiteId, link, request.ShortenLink, ct);
            tags = SocialHashtags.Line(request.Hashtags);
        }
        else
        {
            source = request.Caption ?? "";
        }

        var caption = SocialCaption.Fit(source, link, SocialPostPlanner.XText, tags);
        var chosen = request.Items.Where(item => item.ToX).ToList();
        if (chosen.Count == 0 && caption.Length == 0)
            throw new SocialPublishException("Add a caption or at least one file for X.");

        var files = new List<TelegramOutbound>();
        foreach (var item in chosen)
        {
            var file = await SocialMediaLoader.LoadAsync(context, _host, websiteId, site, item, ct);
            if (!file.IsVideo && file.Bytes.Length > SocialMediaLoader.XImageBytes)
                throw new SocialPublishException("X accepts a photo under 5 MB. Crop it smaller or upload a smaller file.");
            files.Add(file);
        }

        var credentials = new XCredentials(account.ApiKey, account.ApiSecret, account.AccessToken, account.AccessTokenSecret);
        var chunks = files.Count == 0
            ? []
            : SocialPostPlanner.PlanX(files.Select(file => file.IsVideo).ToList());
        string? firstId = null;
        string? previous = null;
        if (chunks.Count == 0)
        {
            firstId = await _api.TweetAsync(credentials, caption, [], null, ct);
        }
        else
        {
            for (var i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var ids = new List<string>();
                for (var n = 0; n < chunk.Count; n++)
                {
                    var file = files[chunk.Start + n];
                    ids.Add(file.IsVideo
                        ? await _api.UploadVideoAsync(credentials, file.Bytes, string.IsNullOrWhiteSpace(file.MimeType) ? "video/mp4" : file.MimeType, ct)
                        : await _api.UploadImageAsync(credentials, file.Bytes, ct));
                }

                var text = i == 0 ? caption : null;
                var id = await _api.TweetAsync(credentials, string.IsNullOrWhiteSpace(text) ? null : text, ids, previous, ct);
                firstId ??= id;
                previous = id;
            }
        }

        if (product)
        {
            var existing = await context.ProductXPublications.FirstOrDefaultAsync(p => p.ProductID == ownerId, ct);
            if (existing is null)
            {
                context.ProductXPublications.Add(new ProductXPublication
                {
                    ProductID = ownerId,
                    TweetId = firstId,
                    PublishedAt = DateTime.UtcNow,
                });
            }
            else
            {
                existing.TweetId = firstId ?? existing.TweetId;
                existing.PublishedAt = DateTime.UtcNow;
            }
        }
        else
        {
            var existing = await context.PostXPublications.FirstOrDefaultAsync(p => p.PostID == ownerId, ct);
            if (existing is null)
            {
                context.PostXPublications.Add(new PostXPublication
                {
                    PostID = ownerId,
                    TweetId = firstId,
                    PublishedAt = DateTime.UtcNow,
                });
            }
            else
            {
                existing.TweetId = firstId ?? existing.TweetId;
                existing.PublishedAt = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync(ct);
    }
}
