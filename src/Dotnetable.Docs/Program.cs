var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Force UTF-8 Content-Type for text assets so Persian/Arabic never mojibake in the browser.
var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider,
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.Name;
        if (path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
        {
            var ct = ctx.Context.Response.ContentType ?? "text/plain";
            if (!ct.Contains("charset", StringComparison.OrdinalIgnoreCase))
                ctx.Context.Response.ContentType = ct + "; charset=utf-8";
        }

        // Avoid stale corrupted caches during local docs work.
        ctx.Context.Response.Headers.CacheControl = "no-cache";
    }
});

// SPA-style fallback so deep links work when opening a docs page directly.
app.MapFallbackToFile("index.html");

app.Run();
