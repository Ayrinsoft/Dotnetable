using System.Text.Json;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Instagram;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Instagram;

public sealed class InstagramPublishService : IInstagramPublishService
{
    private readonly IDbContextFactory<AppDbContext> _contexts;
    private readonly IInstagramGraph _graph;
    private readonly IInstagramMediaHost _host;

    public InstagramPublishService(IDbContextFactory<AppDbContext> contexts, IInstagramGraph graph, IInstagramMediaHost host)
    {
        _contexts = contexts;
        _graph = graph;
        _host = host;
    }

    public async Task<IReadOnlyDictionary<int, string?>> GetLinksAsync(IReadOnlyCollection<int> postIds, CancellationToken ct = default)
    {
        if (postIds.Count == 0) return new Dictionary<int, string?>();
        await using var context = await _contexts.CreateDbContextAsync(ct);
        return await context.PostInstagramPublications.AsNoTracking()
            .Where(p => postIds.Contains(p.PostID))
            .ToDictionaryAsync(p => p.PostID, p => p.Permalink, ct);
    }

    public async Task<string?> PublishAsync(int postId, InstagramPublishRequest request, CancellationToken ct = default)
    {
        await using var context = await _contexts.CreateDbContextAsync(ct);
        var post = await context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.PostID == postId, ct)
            ?? throw new InstagramException("Post was not found.");
        if (await context.PostInstagramPublications.AnyAsync(p => p.PostID == postId, ct))
            throw new InstagramException("This post was already sent to Instagram.");

        var account = await context.WebsiteInstagramAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.WebsiteID == post.WebsiteID && a.IsActive, ct)
            ?? throw new InstagramException("Connect an Instagram account on this page first.");
        var siteAddress = await context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == post.WebsiteID)
            .Select(w => w.WebsiteAddress)
            .FirstOrDefaultAsync(ct);

        var caption = request.UsePrimaryExcerpt ? post.Excerpt ?? "" : request.Caption ?? "";
        var hosted = new List<string>();
        string? outbox = null;
        try
        {
            var slides = new List<PreparedSlide>();
            foreach (var item in request.Items)
            {
                var url = await MaterializeAsync(context, post.WebsiteID, siteAddress, item, hosted, ct);
                slides.Add(new PreparedSlide { Url = url, IsVideo = item.Kind == InstagramMediaKind.Video });
            }

            string? videoCover = null;
            if (request.VideoCover is not null)
            {
                request.VideoCover.Kind = InstagramMediaKind.Image;
                videoCover = await MaterializeAsync(context, post.WebsiteID, siteAddress, request.VideoCover, hosted, ct);
            }

            var plan = InstagramPublishPlanner.Plan(caption, slides, request.CoverIndex, videoCover);
            outbox = await InstagramOutbox.WriteAsync(postId, plan, ct);

            var creationId = await CreateContainerAsync(account, plan, ct);
            var mediaId = await _graph.PublishAsync(account.IgUserId, account.AccessToken, creationId, ct);
            string? permalink = null;
            try
            {
                permalink = await _graph.GetPermalinkAsync(mediaId, account.AccessToken, ct);
            }
            catch (InstagramException)
            {
                permalink = null;
            }

            context.PostInstagramPublications.Add(new PostInstagramPublication
            {
                PostID = postId,
                IgMediaId = mediaId,
                Permalink = permalink,
                PublishedAt = DateTime.UtcNow,
            });
            await context.SaveChangesAsync(ct);
            return permalink;
        }
        finally
        {
            await _host.DeleteHostedAsync(hosted, CancellationToken.None);
            InstagramOutbox.Delete(outbox);
        }
    }

    private async Task<string> CreateContainerAsync(WebsiteInstagramAccount account, InstagramDelivery plan, CancellationToken ct)
    {
        if (plan.IsReel)
        {
            var id = await _graph.CreateVideoAsync(account.IgUserId, account.AccessToken, plan.Slides[0].Url, plan.Caption, carouselItem: false, plan.VideoCoverUrl, ct);
            await _graph.WaitUntilReadyAsync(id, account.AccessToken, video: true, ct);
            return id;
        }

        if (!plan.IsCarousel)
        {
            var only = plan.Slides[0];
            var id = only.IsVideo
                ? await _graph.CreateVideoAsync(account.IgUserId, account.AccessToken, only.Url, plan.Caption, false, null, ct)
                : await _graph.CreateImageAsync(account.IgUserId, account.AccessToken, only.Url, plan.Caption, false, ct);
            await _graph.WaitUntilReadyAsync(id, account.AccessToken, only.IsVideo, ct);
            return id;
        }

        var children = new List<string>();
        foreach (var slide in plan.Slides)
        {
            var child = slide.IsVideo
                ? await _graph.CreateVideoAsync(account.IgUserId, account.AccessToken, slide.Url, null, true, null, ct)
                : await _graph.CreateImageAsync(account.IgUserId, account.AccessToken, slide.Url, null, true, ct);
            await _graph.WaitUntilReadyAsync(child, account.AccessToken, slide.IsVideo, ct);
            children.Add(child);
        }

        var parent = await _graph.CreateCarouselAsync(account.IgUserId, account.AccessToken, children, plan.Caption, ct);
        await _graph.WaitUntilReadyAsync(parent, account.AccessToken, plan.Slides.Any(s => s.IsVideo), ct);
        return parent;
    }

    private async Task<string> MaterializeAsync(
        AppDbContext context, int websiteId, string? siteAddress, InstagramMediaItem item, List<string> hosted, CancellationToken ct)
    {
        var image = item.Kind != InstagramMediaKind.Video;
        if (item.Content is { Length: > 0 })
        {
            var url = image
                ? await _host.HostImageAsync(websiteId, item.Content, item.FileName ?? "instagram.jpg", item.Crop, ct)
                : await _host.HostVideoAsync(websiteId, item.Content, item.FileName ?? "instagram.mp4", item.MimeType, ct);
            hosted.Add(url);
            return RequirePublic(url);
        }

        if (item.FileRecordId is int fileId)
        {
            var file = await context.FileRecords.AsNoTracking()
                .FirstOrDefaultAsync(f => f.FileRecordID == fileId && f.WebsiteID == websiteId && !f.IsDeleted, ct)
                ?? throw new InstagramException("One of the selected files is no longer in the library.");
            var source = RequirePublic(InstagramUrls.Absolute(file.CNDUrl, siteAddress));
            if (!image) return source;

            var bytes = await _host.DownloadAsync(source, ct);
            var hostedUrl = await _host.HostImageAsync(websiteId, bytes, file.OriginalFileName, item.Crop, ct);
            hosted.Add(hostedUrl);
            return RequirePublic(hostedUrl);
        }

        var linked = RequirePublic(InstagramUrls.Absolute(item.Url, siteAddress));
        if (image && item.Crop is not null)
        {
            var bytes = await _host.DownloadAsync(linked, ct);
            var hostedUrl = await _host.HostImageAsync(websiteId, bytes, "instagram.jpg", item.Crop, ct);
            hosted.Add(hostedUrl);
            return RequirePublic(hostedUrl);
        }

        return linked;
    }

    private static string RequirePublic(string? url)
    {
        if (!InstagramUrls.IsPublicHttps(url))
            throw new InstagramException("Instagram can only fetch a public https address. Localhost and a private network will not work.");
        return url!;
    }
}

internal static class InstagramOutbox
{
    public static async Task<string> WriteAsync(int postId, InstagramDelivery plan, CancellationToken ct)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotnetable-instagram", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(new
        {
            postId,
            caption = plan.Caption,
            reel = plan.IsReel,
            carousel = plan.IsCarousel,
            videoCover = plan.VideoCoverUrl,
            items = plan.Slides.Select(s => new { url = s.Url, video = s.IsVideo }),
        });
        await File.WriteAllTextAsync(Path.Combine(dir, "publish.json"), json, ct);
        return dir;
    }

    public static void Delete(string? dir)
    {
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;
        try
        {
            Directory.Delete(dir, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
