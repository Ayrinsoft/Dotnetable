using Dotnetable.Hosting;
using Dotnetable.Web.Infrastructure;
using Dotnetable.Web.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Machine-local overrides (gitignored), same convention as Admin/API: holds the real
// Api:BaseUrl + Api:WebsiteKey for this deployment so they never live in source control.
builder.Configuration.AddJsonFile("localsettings.json", optional: true, reloadOnChange: true);

builder.Host.UseDotnetableLogging("Web");

builder.Services
    .AddControllersWithViews(options =>
    {
        // Every state-changing form post is antiforgery-checked by default rather than per-action.
        // The storefront posts real forms for checkout, wallet withdrawal and cart edits; opting in
        // action by action means the one that gets forgotten is the one that matters.
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    });

// The storefront's login, register, cart and address flows post JSON from script, and a fetch has no
// form to read a hidden token from. The token is published as a meta tag by _Layout and sent back in
// this header; without a matching header name, the global filter above would reject every one of them.
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

// Runtime compilation recompiles .cshtml on every change and needs the source on the server. That is
// a development convenience; in production it costs startup time and memory and ships the templates.
if (builder.Environment.IsDevelopment())
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new ThemeViewLocationExpander());
});

builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddSingleton<WebLocalizationService>();

// Short-TTL read-through cache for ApiClient's public content reads (menu/categories/pages/posts).
builder.Services.AddMemoryCache();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BearerTokenHandler>();
builder.Services.AddTransient<CartSessionHandler>();

builder.Services.AddDotnetableRateLimiting(
    trustForwardedFor: builder.Configuration.GetValue("Hosting:BehindReverseProxy", false));

builder.Services.AddDotnetableDataProtection(
    builder.Configuration, builder.Environment.ContentRootPath, "Web");

builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl is not configured."));

    // Per-site key (the Website.AuthCode of this deployment). The API resolves the website
    // from it — e.g. to scope customer self-registration. Optional so unconfigured dev
    // instances still start; registration just won't have a website to attach to.
    var websiteKey = builder.Configuration["Api:WebsiteKey"];
    if (!string.IsNullOrWhiteSpace(websiteKey))
        client.DefaultRequestHeaders.Add("X-Website-Key", websiteKey);

    // Pin this storefront to the contract it was built against. New API versions can ship
    // without changing URLs; old Web deployments keep hitting 1.0.
    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Version", "1.0");
})
.AddHttpMessageHandler<BearerTokenHandler>()
.AddHttpMessageHandler<CartSessionHandler>();

builder.Services.AddScoped<ContentShortcodeProcessor>();

var app = builder.Build();

if (builder.Configuration.GetValue("Hosting:BehindReverseProxy", false))
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Renders the themed 404/500 views for status codes that never reached an action, instead of the
// blank body Kestrel returns by default.
app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseDotnetableRequestLogging();

// The storefront renders admin-authored HTML, so framing and MIME sniffing are both worth denying.
// No CSP header yet: themes are user-supplied and may carry their own inline scripts, so a policy
// set here would silently break third-party themes — it belongs in a theme's own manifest.
app.UseDotnetableSecurityHeaders();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthorization();

// After routing: any GET that still 404s is offered to the redirect-rule resolver.
app.UseMiddleware<Dotnetable.Web.Infrastructure.RedirectMiddleware>();

// Friendly CMS page URLs: /page/{slug}
app.MapControllerRoute(
    name: "cms-page",
    pattern: "page/{slug}",
    defaults: new { controller = "Page", action = "View" });

app.MapControllerRoute(
    name: "price-lists",
    pattern: "price-lists",
    defaults: new { controller = "PriceList", action = "Index" });

app.MapControllerRoute(
    name: "price-list-detail",
    pattern: "price-lists/{slug}",
    defaults: new { controller = "PriceList", action = "Detail" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
