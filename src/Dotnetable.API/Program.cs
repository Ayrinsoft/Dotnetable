using System.Data.Common;
using System.Security.Claims;
using System.Text;
using Dotnetable.API.Auth;
using Dotnetable.API.Cors;
using Dotnetable.API.Versioning;
using Dotnetable.Application.Authorization;
using Dotnetable.Hosting;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Machine-local overrides (gitignored): the real connection string and secrets for this deployment.
builder.Configuration.AddJsonFile("localsettings.json", optional: true, reloadOnChange: true);

// Outside Development, refuse to boot on the placeholder JWT/sync secrets that ship in
// appsettings.json. They are public in the repository, so a deployment still holding one lets
// anybody mint tokens — see StartupValidation for why failing the boot is the safer outcome.
StartupValidation.ValidateProductionSecrets(
    builder.Configuration, builder.Environment, "Jwt:SigningKey", "Internal:SyncSecret");

builder.Host.UseDotnetableLogging("Api");

builder.Services.AddControllers();

// Shared bot check for every public write endpoint (registration, reset, reviews, questions, forms).
builder.Services.AddScoped<Dotnetable.API.Auth.CaptchaGuard>();

// ── JWT bearer authentication for website clients (and any token-based caller) ──
var jwt = new JwtSettings();
builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwt);

// In Development an unset key still needs *something* to construct the validator with; production
// never reaches here with an empty key because ValidateProductionSecrets already threw.
var signingKey = string.IsNullOrWhiteSpace(jwt.SigningKey)
    ? "development-only-unconfigured-signing-key-0123456789"
    : jwt.SigningKey;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            RoleClaimType = ClaimTypes.Role,
        };
    });

builder.Services.AddAuthorization(ApiAuthorization.Register);

builder.Services.AddDotnetableApiVersioning();

// Per-IP limits on credential, delivery, checkout and upload endpoints. The per-account limits live
// in WebsiteClientAuthService; both are needed — see Dotnetable.Hosting.RateLimiting.
builder.Services.AddDotnetableRateLimiting(
    trustForwardedFor: builder.Configuration.GetValue("Hosting:BehindReverseProxy", false));

builder.Services.AddDotnetableDataProtection(
    builder.Configuration, builder.Environment.ContentRootPath, "Api");

// CORS for browser-based front-ends (the React SPA in serverless mode calls the API directly).
//
// The allowed-origin list comes from the Websites table (WebsiteAddress) instead of a static
// Cors:AllowedOrigins config entry that would have to be kept in sync by hand — every storefront the
// API talks to is already a Website row. Cors:AllowedOrigins is still honored on top for origins that
// are not a Website row (e.g. a separate marketing site or SPA host). See DynamicCorsOriginProvider.
builder.Services.AddCors();
builder.Services.AddSingleton<DynamicCorsOriginProvider>();
builder.Services.AddSingleton<ICorsPolicyProvider, DynamicCorsPolicyProvider>();
builder.Services.AddHostedService<CorsOriginRefreshService>();

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);
builder.Services.AddDotnetableHealthChecks<AppDbContext>();

var app = builder.Build();

// Warm the CORS origin snapshot before accepting requests — CorsOriginRefreshService's own first tick
// runs concurrently with host startup, which could otherwise let an early request see an empty list
// (e.g. right after a fresh deploy). Harmless no-op before Setup has configured a database.
await app.Services.GetRequiredService<DynamicCorsOriginProvider>().RefreshAsync();

// Behind a proxy the remote IP is the proxy's, which would put every visitor in one rate-limit
// bucket and log one address for everyone. Only honoured when explicitly enabled.
if (builder.Configuration.GetValue("Hosting:BehindReverseProxy", false))
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    });
}

// Surfaces DB connectivity/config failures (bad connection string, unreachable server, cert
// mismatch, etc.) as a clear 503 instead of a cryptic RetryLimitExceededException/SqlException
// stack trace — this is exactly what a misconfigured localsettings.json produces.
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    if (error is DbException or RetryLimitExceededException)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "database_unavailable",
            message = "Could not connect to the database. Check the Database section in this app's localsettings.json (provider, connection string, credentials).",
            detail = app.Environment.IsDevelopment() ? error.Message : null,
        });
        return;
    }

    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new
    {
        error = "server_error",
        detail = app.Environment.IsDevelopment() ? error?.Message : null,
    });
}));

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseDotnetableRequestLogging();

// The API serves uploaded media whose MIME type the uploader chose, so nosniff is load-bearing here
// rather than decorative. No CSP: this host returns JSON and files, never a document to script into.
app.UseDotnetableSecurityHeaders();

app.UseCors();
app.UseDotnetableApiVersionHeaders();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapDotnetableHealthChecks();

// This API has no landing page of its own — hitting its bare root previously 404'd, which reads as
// "broken" to anyone (a load balancer, a curious human) that just pings the address. Answer the same
// liveness check /health/live does instead of a 404.
app.MapHealthChecks("/", new HealthCheckOptions { Predicate = check => check.Tags.Contains("live") })
    .AllowAnonymous();

app.Run();
