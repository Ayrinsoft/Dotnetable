using Dotnetable.Web.Infrastructure;
using Dotnetable.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new ThemeViewLocationExpander());
});

builder.Services.AddSingleton<IThemeService, ThemeService>();
builder.Services.AddSingleton<WebLocalizationService>();

builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl is not configured."));
    client.DefaultRequestHeaders.Add("X-Api-Key",
        builder.Configuration["Api:Key"]
        ?? throw new InvalidOperationException("Api:Key is not configured."));
});

builder.Services.AddHttpContextAccessor();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
