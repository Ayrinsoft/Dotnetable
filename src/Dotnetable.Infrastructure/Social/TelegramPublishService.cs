using Dotnetable.Application.DTOs;
using Dotnetable.Application.Instagram;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Instagram;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Social;

public sealed class TelegramPublishService
{
    private readonly IDbContextFactory<AppDbContext> _contexts;
    private readonly IInstagramMediaHost _host;
    private readonly ITelegramBot _bot;

    public TelegramPublishService(IDbContextFactory<AppDbContext> contexts, IInstagramMediaHost host, ITelegramBot bot)
    {
        _contexts = contexts;
        _host = host;
        _bot = bot;
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
        if (product)
        {
            var row = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductID == ownerId, ct)
                ?? throw new SocialPublishException("Product was not found.");
            websiteId = row.WebsiteID;
            excerpt = row.ShortDescription ?? "";
        }
        else
        {
            var post = await context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.PostID == ownerId, ct)
                ?? throw new SocialPublishException("Post was not found.");
            websiteId = post.WebsiteID;
            excerpt = post.Excerpt ?? "";
        }

        using var proxy = SocialProxyLease.Open(await SocialProxyLease.ReadAsync(context, websiteId, ct));
        var account = await context.WebsiteTelegramAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.WebsiteID == websiteId && a.IsActive, ct)
            ?? throw new SocialPublishException("Connect a Telegram bot on this page first.");
        var site = await context.Websites.AsNoTracking().Where(w => w.WebsiteID == websiteId).Select(w => w.WebsiteAddress).FirstOrDefaultAsync(ct);
        var caption = request.UsePrimaryExcerpt ? excerpt : request.Caption ?? "";
        var chosen = request.Items.Where(item => item.ToTelegram).ToList();
        if (chosen.Count == 0 && string.IsNullOrWhiteSpace(caption))
            throw new SocialPublishException("Add a caption or at least one file for Telegram.");

        var files = new List<TelegramOutbound>();
        foreach (var item in chosen)
            files.Add(await SocialMediaLoader.LoadAsync(context, _host, websiteId, site, item, ct));

        string? messageId = null;
        var textParts = SocialPostPlanner.SplitText(caption, SocialPostPlanner.TelegramMessage);
        if (files.Count == 0)
        {
            if (textParts.Count == 0) throw new SocialPublishException("Add a caption or at least one file for Telegram.");
            foreach (var part in textParts)
                messageId = await _bot.SendTextAsync(account.BotToken, account.ChatId, part, ct) ?? messageId;
        }
        else
        {
            var mediaCaption = caption.Length <= SocialPostPlanner.TelegramCaption ? caption : null;
            if (mediaCaption is null)
            {
                foreach (var part in textParts)
                    messageId = await _bot.SendTextAsync(account.BotToken, account.ChatId, part, ct) ?? messageId;
            }

            var albums = SocialPostPlanner.PlanTelegram(files.Count);
            for (var album = 0; album < albums.Count; album++)
            {
                var (start, count) = albums[album];
                var slice = files.GetRange(start, count);
                var albumCaption = album == 0 ? mediaCaption : null;
                messageId = slice.Count switch
                {
                    1 when !slice[0].IsVideo => await _bot.SendPhotoAsync(account.BotToken, account.ChatId, slice[0], albumCaption, ct),
                    1 => await _bot.SendVideoAsync(account.BotToken, account.ChatId, slice[0], albumCaption, ct),
                    _ => await _bot.SendAlbumAsync(account.BotToken, account.ChatId, slice, albumCaption, ct),
                } ?? messageId;
            }
        }

        if (product)
        {
            var existing = await context.ProductTelegramPublications.FirstOrDefaultAsync(p => p.ProductID == ownerId, ct);
            if (existing is null)
            {
                context.ProductTelegramPublications.Add(new ProductTelegramPublication
                {
                    ProductID = ownerId,
                    MessageId = messageId,
                    PublishedAt = DateTime.UtcNow,
                });
            }
            else
            {
                existing.MessageId = messageId ?? existing.MessageId;
                existing.PublishedAt = DateTime.UtcNow;
            }
        }
        else
        {
            var existing = await context.PostTelegramPublications.FirstOrDefaultAsync(p => p.PostID == ownerId, ct);
            if (existing is null)
            {
                context.PostTelegramPublications.Add(new PostTelegramPublication
                {
                    PostID = ownerId,
                    MessageId = messageId,
                    PublishedAt = DateTime.UtcNow,
                });
            }
            else
            {
                existing.MessageId = messageId ?? existing.MessageId;
                existing.PublishedAt = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync(ct);
    }
}
