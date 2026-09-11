using Dotnetable.Admin.Auth;
using Dotnetable.Hosting;
using Dotnetable.Infrastructure.Data;
using Microsoft.AspNetCore.HttpOverrides;
using Dotnetable.Admin.Localization;
using Dotnetable.Admin.Middleware;
using Dotnetable.Admin.Services;
using Dotnetable.Application;
using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Machine-local overrides (gitignored): the real connection string and secrets for this deployment.
builder.Configuration.AddJsonFile("localsettings.json", optional: true, reloadOnChange: true);

// The Admin panel pushes cache invalidation to the API with Internal:SyncSecret; a deployment still
// holding the placeholder committed to source control lets anyone flush the API cache at will.
StartupValidation.ValidateProductionSecrets(
    builder.Configuration, builder.Environment, "Internal:SyncSecret");

builder.Host.UseDotnetableLogging("Admin");

// Brand name shown in titles/chrome — single source of truth, overridable via configuration.
AppBranding.Name = builder.Configuration["Branding:AppName"] ?? AppBranding.Name;

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);
builder.Services.AddScoped<IPageLocalizer, PageLocalizer>();
builder.Services.AddScoped<IAuthLanguageResolver, AuthLanguageResolver>();
builder.Services.AddScoped<AdminUiModeState>();
builder.Services.AddScoped<AdminNavSurfaceState>();

// Admin writes content directly against the DB (bypassing the API), so its own cache invalidation
// never reaches the API process on its own — push it over HTTP. Overrides the no-op default that
// AddInfrastructure registers.
builder.Services.AddHttpClient<RemoteCacheInvalidationNotifier>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl is not configured."));
});
builder.Services.AddScoped<ICacheInvalidationNotifier>(sp =>
    sp.GetRequiredService<RemoteCacheInvalidationNotifier>());
builder.Services.AddScoped<SyncSecretProvisioner>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(AdminPolicies.Register);

// Expands the slim sign-in cookie's PolicyID claim into role claims per request instead of storing
// them in the cookie itself — see MemberClaims.BuildForCookie.
builder.Services.AddScoped<IClaimsTransformation, PolicyRoleClaimsTransformation>();

// Holds the half-finished sign-in between the password step and the authenticator-code step.
// Nothing in it is an authentication on its own — see LoginModel.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.Name = "dn-admin-session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddDotnetableRateLimiting(
    trustForwardedFor: builder.Configuration.GetValue("Hosting:BehindReverseProxy", false));

// Without persisted keys every deploy signs every administrator out and invalidates every
// outstanding antiforgery token.
builder.Services.AddDotnetableDataProtection(
    builder.Configuration, builder.Environment.ContentRootPath, "Admin");

builder.Services.AddDotnetableHealthChecks<AppDbContext>();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddMudServices();
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();
// Used by the media download proxy (and any other ad-hoc server-side HTTP calls).
builder.Services.AddHttpClient();

var app = builder.Build();

// When the panel boots with a configured database, apply any pending schema updates that shipped
// with a new build. Additive migrations upgrade a running install without manual steps.
using (var scope = app.Services.CreateScope())
{
    var configStore = scope.ServiceProvider.GetRequiredService<IDatabaseConfigStore>();
    if (configStore.IsConfigured)
    {
        try
        {
            var updater = scope.ServiceProvider.GetRequiredService<IDatabaseUpdateService>();
            var pending = await updater.GetPendingUpdatesAsync();
            if (pending.Count > 0)
                await updater.ApplyUpdatesAsync();

            // Top up catalog permissions a prior version did not seed, and grant them to Administrators.
            var setup = scope.ServiceProvider.GetRequiredService<ISetupService>();
            await setup.SyncRoleCatalogAsync();

            // One-time (idempotent) fix for slugs saved verbatim from a raw title before slug
            // sanitizing existed — re-derives anything not already URL-safe.
            await setup.ReslugifyContentAsync();
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Applying database updates on startup failed.");
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (builder.Configuration.GetValue("Hosting:BehindReverseProxy", false))
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseDotnetableRequestLogging();

// The panel embeds nothing and should never be embedded; framing it is only useful to an attacker
// overlaying a transparent page on top of a real admin action.
app.UseDotnetableSecurityHeaders();

app.UseSession();
app.UseRateLimiter();
app.UseSetupRedirect();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Theme package screenshots (WordPress-style). Files live under Themes:RootPath.
app.MapGet("/theme-files/{websiteId:int}/{slug}/screenshot", async (
    int websiteId,
    string slug,
    IThemeService themes,
    CancellationToken ct) =>
{
    var path = themes.GetScreenshotPath(websiteId, slug);
    if (path is null || !System.IO.File.Exists(path))
        return Results.NotFound();

    var contentType = path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png"
        : path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
            ? "image/jpeg"
            : "application/octet-stream";
    var bytes = await System.IO.File.ReadAllBytesAsync(path, ct);
    return Results.File(bytes, contentType);
}).RequireAuthorization();

// Same-origin media download with Content-Disposition: attachment.
// Proxies the public CDN/API URL so the browser can save images/videos/docs without CORS issues
// or opening a new tab.
app.MapGet("/media/download/{fileId:int}", async (
    int fileId,
    HttpContext httpContext,
    IFileService fileService,
    IConfiguration config,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct) =>
{
    var file = await fileService.GetByIdAsync(fileId, ct);
    if (file is null || file.IsDeleted)
        return Results.NotFound();

    var publicUrl = !string.IsNullOrWhiteSpace(file.CNDUrl) ? file.CNDUrl!.Trim()
        : !string.IsNullOrWhiteSpace(file.ThumbnailCDN) ? file.ThumbnailCDN!.Trim()
        : null;
    if (string.IsNullOrWhiteSpace(publicUrl))
        return Results.NotFound();

    // Local storage often stores a relative API path (/api/files/{websiteId}/{name}).
    if (publicUrl.StartsWith('/'))
    {
        var apiBase = (config["Api:BaseUrl"] ?? "").TrimEnd('/');
        if (string.IsNullOrEmpty(apiBase))
            return Results.Problem("Api:BaseUrl is not configured; cannot resolve relative media URL.");
        publicUrl = apiBase + publicUrl;
    }

    var client = httpClientFactory.CreateClient();
    HttpResponseMessage upstream;
    try
    {
        upstream = await client.GetAsync(publicUrl, HttpCompletionOption.ResponseHeadersRead, ct);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Could not fetch media from storage: {ex.Message}");
    }

    if (!upstream.IsSuccessStatusCode)
    {
        var status = (int)upstream.StatusCode;
        upstream.Dispose();
        return Results.StatusCode(status);
    }

    // Keep the upstream response alive until the body stream is fully written to the client.
    httpContext.Response.RegisterForDispose(upstream);

    var stream = await upstream.Content.ReadAsStreamAsync(ct);
    var mime = string.IsNullOrWhiteSpace(file.MimeType) ? "application/octet-stream" : file.MimeType!;
    var fileName = string.IsNullOrWhiteSpace(file.OriginalFileName)
        ? "download"
        : Path.GetFileName(file.OriginalFileName);

    return Results.File(stream, contentType: mime, fileDownloadName: fileName);
}).RequireAuthorization();

app.MapDotnetableHealthChecks();
app.MapRazorPages();
app.MapRazorComponents<Dotnetable.Admin.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
