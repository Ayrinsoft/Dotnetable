using Dotnetable.Application.DTOs;
using Dotnetable.Application.Instagram;
using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Social;

public sealed class SocialPublishService : ISocialPublishService
{
    private readonly IDbContextFactory<AppDbContext> _contexts;
    private readonly IInstagramPublishService _instagram;
    private readonly TelegramPublishService _telegram;
    private readonly XPublishService _x;

    public SocialPublishService(
        IDbContextFactory<AppDbContext> contexts,
        IInstagramPublishService instagram,
        TelegramPublishService telegram,
        XPublishService x)
    {
        _contexts = contexts;
        _instagram = instagram;
        _telegram = telegram;
        _x = x;
    }

    public async Task<IReadOnlyDictionary<int, SocialSentFlags>> GetSentAsync(IReadOnlyCollection<int> postIds, CancellationToken ct = default)
    {
        if (postIds.Count == 0) return new Dictionary<int, SocialSentFlags>();
        await using var context = await _contexts.CreateDbContextAsync(ct);
        var instagram = await context.PostInstagramPublications.AsNoTracking()
            .Where(p => postIds.Contains(p.PostID)).Select(p => p.PostID).ToListAsync(ct);
        var telegram = await context.PostTelegramPublications.AsNoTracking()
            .Where(p => postIds.Contains(p.PostID)).Select(p => p.PostID).ToListAsync(ct);
        var x = await context.PostXPublications.AsNoTracking()
            .Where(p => postIds.Contains(p.PostID)).Select(p => p.PostID).ToListAsync(ct);
        return Flags(postIds, instagram, telegram, x);
    }

    private static Dictionary<int, SocialSentFlags> Flags(
        IReadOnlyCollection<int> ids, List<int> instagram, List<int> telegram, List<int> x)
    {
        var ig = instagram.ToHashSet();
        var tg = telegram.ToHashSet();
        var xs = x.ToHashSet();
        var result = new Dictionary<int, SocialSentFlags>();
        foreach (var id in ids.Distinct())
        {
            var flags = new SocialSentFlags { Instagram = ig.Contains(id), Telegram = tg.Contains(id), X = xs.Contains(id) };
            if (flags.Instagram || flags.Telegram || flags.X)
                result[id] = flags;
        }

        return result;
    }

    public async Task<IReadOnlyDictionary<int, SocialSentFlags>> GetProductSentAsync(IReadOnlyCollection<int> productIds, CancellationToken ct = default)
    {
        if (productIds.Count == 0) return new Dictionary<int, SocialSentFlags>();
        await using var context = await _contexts.CreateDbContextAsync(ct);
        var instagram = await context.ProductInstagramPublications.AsNoTracking()
            .Where(p => productIds.Contains(p.ProductID)).Select(p => p.ProductID).ToListAsync(ct);
        var telegram = await context.ProductTelegramPublications.AsNoTracking()
            .Where(p => productIds.Contains(p.ProductID)).Select(p => p.ProductID).ToListAsync(ct);
        var x = await context.ProductXPublications.AsNoTracking()
            .Where(p => productIds.Contains(p.ProductID)).Select(p => p.ProductID).ToListAsync(ct);
        return Flags(productIds, instagram, telegram, x);
    }

    public Task<SocialPublishResult> PublishAsync(int postId, InstagramPublishRequest request, CancellationToken ct = default) =>
        RunAsync(request,
            filtered => _instagram.PublishAsync(postId, filtered, ct),
            () => _telegram.PublishAsync(postId, request, ct),
            () => _x.PublishAsync(postId, request, ct));

    public Task<SocialPublishResult> PublishProductAsync(int productId, InstagramPublishRequest request, CancellationToken ct = default) =>
        RunAsync(request,
            filtered => _instagram.PublishProductAsync(productId, filtered, ct),
            () => _telegram.PublishProductAsync(productId, request, ct),
            () => _x.PublishProductAsync(productId, request, ct));

    private static async Task<SocialPublishResult> RunAsync(
        InstagramPublishRequest request,
        Func<InstagramPublishRequest, Task> instagram,
        Func<Task> telegram,
        Func<Task> x)
    {
        if (!request.SendInstagram && !request.SendTelegram && !request.SendX)
            throw new InstagramException("Choose Instagram, Telegram, or X.");

        var errors = new List<string>();
        var sentInstagram = false;
        var sentTelegram = false;
        var sentX = false;
        if (request.SendInstagram)
        {
            try
            {
                await instagram(ForInstagram(request));
                sentInstagram = true;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                errors.Add("Instagram: " + ex.Message);
            }
        }

        if (request.SendTelegram)
        {
            try
            {
                await telegram();
                sentTelegram = true;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                errors.Add("Telegram: " + ex.Message);
            }
        }

        if (request.SendX)
        {
            try
            {
                await x();
                sentX = true;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                errors.Add("X: " + ex.Message);
            }
        }

        return new SocialPublishResult { Instagram = sentInstagram, Telegram = sentTelegram, X = sentX, Errors = errors };
    }

    internal static InstagramPublishRequest ForInstagram(InstagramPublishRequest request)
    {
        var chosen = request.Items.Select((item, index) => (item, index)).Where(pair => pair.item.ToInstagram).ToList();
        if (chosen.Count == 0)
            throw new InstagramException("Choose at least one photo or video for Instagram.");
        var cover = request.CoverIndex;
        var newCover = 0;
        for (var i = 0; i < chosen.Count; i++)
        {
            if (chosen[i].index == cover) newCover = i;
        }

        return new InstagramPublishRequest
        {
            Caption = request.Caption,
            UsePrimaryExcerpt = request.UsePrimaryExcerpt,
            CoverIndex = newCover,
            VideoCover = request.VideoCover,
            SendInstagram = true,
            Hashtags = request.Hashtags,
            ShortenLink = request.ShortenLink,
            SkipAppend = request.SkipAppend,
            Items = chosen.Select(pair => pair.item).ToList(),
        };
    }
}
