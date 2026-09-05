using System.Data.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Dotnetable.Hosting;

/// <summary>
/// Logging and health endpoints. Both exist for the same reason: once the shop is live, the only
/// thing worse than a fault is a fault nobody can see.
/// </summary>
public static class Observability
{
    /// <summary>
    /// Replaces the default console logger with Serilog writing to stdout and to a rolling file.
    ///
    /// <para>The file sink matters for a self-hosted shop with no log aggregator: a crash at 3am has
    /// to leave something on disk to read the next morning. Files roll daily and are capped, so the
    /// log cannot quietly fill the volume the database lives on.</para>
    /// </summary>
    public static IHostBuilder UseDotnetableLogging(this IHostBuilder host, string applicationName) =>
        host.UseSerilog((context, services, configuration) =>
        {
            // "" (not just absent) means "use the default" — appsettings.json ships FilePath as ""
            // so a fresh clone doesn't need to fill it in, so a plain ?? here would pass an empty
            // path straight to Directory.CreateDirectory and throw.
            var configuredPath = context.Configuration["Logging:FilePath"];
            var logDirectory = string.IsNullOrWhiteSpace(configuredPath)
                ? Path.Combine(context.HostingEnvironment.ContentRootPath, "App_Data", "logs")
                : configuredPath;
            Directory.CreateDirectory(logDirectory);

            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", applicationName)
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                .WriteTo.Console()
                .WriteTo.File(
                    Path.Combine(logDirectory, $"{applicationName.ToLowerInvariant()}-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31,
                    fileSizeLimitBytes: 64L * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    shared: true);
        });

    /// <summary>
    /// Request logging: one structured line per request instead of the framework's several, with the
    /// caller and status attached so a spike of 401s or 429s is visible without grepping.
    /// </summary>
    public static IApplicationBuilder UseDotnetableRequestLogging(this IApplicationBuilder app) =>
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, elapsed, ex) =>
                ex is not null || httpContext.Response.StatusCode >= 500 ? LogEventLevel.Error
                : httpContext.Response.StatusCode >= 400 ? LogEventLevel.Warning
                // Health probes fire constantly and would drown everything else.
                : httpContext.Request.Path.StartsWithSegments("/health") ? LogEventLevel.Verbose
                : LogEventLevel.Information;

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString());
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                if (httpContext.User.Identity?.IsAuthenticated == true)
                    diagnosticContext.Set("User", httpContext.User.Identity.Name);
            };
        });

    /// <summary>
    /// Registers a liveness check (the process answers) and a readiness check (the database answers).
    /// Splitting them matters to an orchestrator: a failed readiness probe should take the instance
    /// out of rotation, a failed liveness probe should restart it, and a database outage calls for
    /// the first, never the second.
    /// </summary>
    public static IServiceCollection AddDotnetableHealthChecks<TDbContext>(this IServiceCollection services)
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
            .AddCheck<DatabaseHealthCheck<TDbContext>>("database", tags: new[] { "ready" });

        return services;
    }

    /// <summary>Maps <c>/health/live</c> and <c>/health/ready</c>. Both are anonymous and leak nothing.</summary>
    public static void MapDotnetableHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
        }).AllowAnonymous();

        endpoints.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
        }).AllowAnonymous();
    }
}

/// <summary>
/// Readiness probe: opens a connection through the configured provider. Deliberately reports the
/// failure category only — a probe endpoint is anonymous, so it must never echo a connection string
/// or a server name back to the caller.
/// </summary>
public sealed class DatabaseHealthCheck<TDbContext> : IHealthCheck
    where TDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    private readonly Microsoft.EntityFrameworkCore.IDbContextFactory<TDbContext> _contextFactory;

    public DatabaseHealthCheck(Microsoft.EntityFrameworkCore.IDbContextFactory<TDbContext> contextFactory) =>
        _contextFactory = contextFactory;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);
            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("The database did not accept a connection.");
        }
        catch (DbException)
        {
            return HealthCheckResult.Unhealthy("The database did not accept a connection.");
        }
        catch (Exception)
        {
            return HealthCheckResult.Unhealthy("The database check failed.");
        }
    }
}
