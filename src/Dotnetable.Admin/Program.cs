using Dotnetable.Admin.Auth;
using Dotnetable.Admin.Localization;
using Dotnetable.Admin.Middleware;
using Dotnetable.Admin.Services;
using Dotnetable.Application;
using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Brand name shown in titles/chrome — single source of truth, overridable via configuration.
AppBranding.Name = builder.Configuration["Branding:AppName"] ?? AppBranding.Name;

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);
builder.Services.AddScoped<IPageLocalizer, PageLocalizer>();
builder.Services.AddScoped<IAuthLanguageResolver, AuthLanguageResolver>();
builder.Services.AddScoped<AdminUiModeState>();

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
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddMudServices();
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

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
            {
                app.Logger.LogInformation("Applying {Count} pending database update(s): {Updates}",
                    pending.Count, string.Join(", ", pending));
                await updater.ApplyUpdatesAsync();
            }

            // Top up any permission keys a prior version did not seed (additive, idempotent).
            var setup = scope.ServiceProvider.GetRequiredService<ISetupService>();
            await setup.SyncRoleCatalogAsync();
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
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

app.MapRazorPages();
app.MapRazorComponents<Dotnetable.Admin.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
