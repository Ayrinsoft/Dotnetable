using Dotnetable.Admin.Auth;
using Dotnetable.Admin.Middleware;
using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

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

app.MapRazorPages();
app.MapRazorComponents<Dotnetable.Admin.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
