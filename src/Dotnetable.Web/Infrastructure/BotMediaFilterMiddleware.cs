using System.Text;
using Dotnetable.Web.Services;

namespace Dotnetable.Web.Infrastructure;

/// <summary>
/// Serves crawlers the full page with no links to uploaded files (<see cref="MediaHtml.StripForBots"/>).
/// Every page stays open to bots and in the sitemap; only the storage CDN is kept out of reach, so
/// indexing never costs file bandwidth. People — in-app browsers included — are passed straight
/// through without buffering.
/// </summary>
public sealed class BotMediaFilterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BotMediaFilterMiddleware> _logger;

    public BotMediaFilterMiddleware(RequestDelegate next, ILogger<BotMediaFilterMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var kind = BotDetector.Classify(context);
        if (kind == BotKind.None || !(HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method)))
        {
            await _next(context);
            return;
        }

        var original = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);
        }
        finally
        {
            context.Response.Body = original;
        }

        buffer.Position = 0;
        var isHtml = context.Response.ContentType?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) == true;
        if (!isHtml || buffer.Length == 0 || HttpMethods.IsHead(context.Request.Method))
        {
            await buffer.CopyToAsync(original, context.RequestAborted);
            return;
        }

        string html;
        using (var reader = new StreamReader(buffer, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true))
            html = await reader.ReadToEndAsync(context.RequestAborted);

        try
        {
            html = MediaHtml.StripForBots(html, keepShareImage: kind == BotKind.LinkPreview);
        }
        catch (Exception ex)
        {
            // Never fail a crawl over this: fall back to the unfiltered page.
            _logger.LogWarning(ex, "Could not strip file links for a bot request to {Path}.", context.Request.Path);
        }

        var bytes = Encoding.UTF8.GetBytes(html);
        context.Response.ContentLength = bytes.Length;
        // A cache in front of the site must never hand this stripped copy to a person.
        context.Response.Headers.CacheControl = "private, no-store";
        await original.WriteAsync(bytes, context.RequestAborted);
    }
}
