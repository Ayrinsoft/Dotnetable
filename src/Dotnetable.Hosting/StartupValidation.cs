using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dotnetable.Hosting;

/// <summary>
/// Startup-time guards and infrastructure that only matter once the app is not running on a laptop.
/// </summary>
public static class StartupValidation
{
    /// <summary>
    /// The placeholder secrets that ship in <c>appsettings.json</c> so a fresh clone starts. Every
    /// one of them is public in the repository, so a deployment still holding one is not "insecure
    /// by degree" — it is unauthenticated.
    /// </summary>
    private static readonly string[] KnownDevelopmentSecrets =
    {
        "dev-only-change-me-0123456789abcdef0123456789",
        "dev-only-change-me-cache-sync-secret",
    };

    /// <summary>
    /// Refuses to start outside Development when a required secret is missing or is still the value
    /// committed to source control.
    ///
    /// <para>This replaces the previous behaviour of substituting a 32-zero-byte JWT key so "startup
    /// never crashes": a misconfigured production deployment came up healthy and signed tokens with
    /// a key anyone could guess, which meant any visitor could mint an administrator token. Failing
    /// the boot is the strictly safer outcome — a service that will not start gets noticed, a
    /// service that starts wide open does not.</para>
    /// </summary>
    /// <param name="requiredKeys">Configuration keys that must hold a real value, e.g. <c>Jwt:SigningKey</c>.</param>
    public static void ValidateProductionSecrets(
        IConfiguration configuration,
        IHostEnvironment environment,
        params string[] requiredKeys)
    {
        if (environment.IsDevelopment()) return;

        var problems = new List<string>();

        foreach (var key in requiredKeys)
        {
            var value = configuration[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                problems.Add($"'{key}' is not configured.");
                continue;
            }

            if (KnownDevelopmentSecrets.Contains(value, StringComparer.Ordinal))
                problems.Add($"'{key}' is still the placeholder value committed to source control.");

            // HMAC-SHA256 needs a key at least as long as its output to deliver its nominal strength.
            if (key.EndsWith("SigningKey", StringComparison.OrdinalIgnoreCase) && value.Length < 32)
                problems.Add($"'{key}' must be at least 32 characters.");
        }

        if (problems.Count == 0) return;

        throw new InvalidOperationException(
            "Refusing to start with insecure configuration:" + Environment.NewLine +
            string.Join(Environment.NewLine, problems.Select(p => "  - " + p)) + Environment.NewLine +
            "Set these via environment variables (e.g. Jwt__SigningKey) or localsettings.json before deploying.");
    }

    /// <summary>
    /// Persists data-protection keys to disk so they survive a restart.
    ///
    /// <para>Without this, ASP.NET Core keeps the key ring in memory: every deploy or app-pool
    /// recycle signs everyone out and invalidates every outstanding antiforgery token, and two
    /// instances behind a load balancer cannot read each other's cookies at all. The directory must
    /// be on storage that outlives the container — mount it as a volume.</para>
    /// </summary>
    public static IServiceCollection AddDotnetableDataProtection(
        this IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath,
        string applicationName)
    {
        var configured = configuration["DataProtection:KeyPath"];
        var keyPath = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(contentRootPath, "App_Data", "keys")
            : configured;

        Directory.CreateDirectory(keyPath);

        services.AddDataProtection()
            // A shared application name would let the three apps read each other's protected payloads;
            // they are separate trust boundaries, so each gets its own isolation ring.
            .SetApplicationName($"Dotnetable.{applicationName}")
            .PersistKeysToFileSystem(new DirectoryInfo(keyPath));

        return services;
    }
}
