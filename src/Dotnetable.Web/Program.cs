using Dotnetable.Web.Infrastructure;
using Dotnetable.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Machine-local overrides (gitignored), same convention as Admin/API: holds the real
// Api:BaseUrl + Api:WebsiteKey for this deployment so they never live in source control.
builder.Configuration.AddJsonFile("localsettings.json", optional: true, reloadOnChange: true);

builder.Services
    .AddControllersWithViews()
    .AddRazorRuntimeCompilation();

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
})
.AddHttpMessageHandler<BearerTokenHandler>()
.AddHttpMessageHandler<CartSessionHandler>();

builder.Services.AddScoped<ContentShortcodeProcessor>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// After routing: any GET that still 404s is offered to the redirect-rule resolver.
app.UseMiddleware<Dotnetable.Web.Infrastructure.RedirectMiddleware>();

// Friendly CMS page URLs: /page/{slug}
app.MapControllerRoute(
    name: "cms-page",
    pattern: "page/{slug}",
    defaults: new { controller = "Page", action = "View" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
