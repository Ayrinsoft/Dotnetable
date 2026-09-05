using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
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

    private const string LocalSettingsFileName = "localsettings.json";

    /// <summary>
    /// Outside Development, replaces any required secret that is missing or still the placeholder
    /// committed to source control with a freshly generated random value, persisted into
    /// <c>localsettings.json</c> (created next to the app if it does not exist yet) so the same
    /// value survives the next restart.
    ///
    /// <para>This used to throw instead: a misconfigured production deployment silently booting
    /// with a public, guessable secret let any visitor mint an administrator token. Generating and
    /// persisting a real secret on first run keeps that fixed while removing the manual step —
    /// deploying just works, and the generated value sticks around in <c>localsettings.json</c>
    /// rather than changing (and invalidating every session/token) on every restart.</para>
    ///
    /// <para>A secret that <em>is</em> configured but fails validation (e.g. a custom signing key
    /// shorter than 32 characters) is left alone and still fails the boot — that is a deliberate
    /// value someone set, not an absent one, and silently overwriting it would be surprising.</para>
    /// </summary>
    /// <param name="requiredKeys">Configuration keys that must hold a real value, e.g. <c>Jwt:SigningKey</c>.</param>
    public static void ValidateProductionSecrets(
        IConfiguration configuration,
        IHostEnvironment environment,
        params string[] requiredKeys)
    {
        if (environment.IsDevelopment()) return;

        var toGenerate = new List<string>();
        var problems = new List<string>();

        foreach (var key in requiredKeys)
        {
            var value = configuration[key];
            var missingOrPlaceholder = string.IsNullOrWhiteSpace(value) ||
                KnownDevelopmentSecrets.Contains(value, StringComparer.Ordinal);

            if (missingOrPlaceholder)
            {
                toGenerate.Add(key);
                continue;
            }

            // HMAC-SHA256 needs a key at least as long as its output to deliver its nominal strength.
            if (key.EndsWith("SigningKey", StringComparison.OrdinalIgnoreCase) && value!.Length < 32)
                problems.Add($"'{key}' must be at least 32 characters.");
        }

        if (toGenerate.Count > 0)
        {
            foreach (var key in toGenerate)
            {
                if (!TrySetLocalSetting(configuration, environment, key, GenerateSecretValue(), out var error))
                {
                    problems.Add(error!);
                    continue;
                }

                Console.WriteLine(
                    $"[StartupValidation] Generated and saved '{key}' to localsettings.json because no real " +
                    "value was configured. Back this file up — losing it invalidates every signed-in session " +
                    "and issued token.");
            }
        }

        if (problems.Count == 0) return;

        throw new InvalidOperationException(
            "Refusing to start with insecure configuration:" + Environment.NewLine +
            string.Join(Environment.NewLine, problems.Select(p => "  - " + p)) + Environment.NewLine +
            "Set these via environment variables (e.g. Jwt__SigningKey) or localsettings.json before deploying.");
    }

    /// <summary>Whether <paramref name="value"/> is one of the placeholder secrets shipped in source control.</summary>
    public static bool IsPlaceholderSecret(string? value) =>
        !string.IsNullOrWhiteSpace(value) && KnownDevelopmentSecrets.Contains(value, StringComparer.Ordinal);

    /// <summary>32 random bytes as base64 — 44 characters, well past the 32-character minimum any of these keys need.</summary>
    public static string GenerateSecretValue() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    /// <summary>
    /// Merges a single value into <c>localsettings.json</c> on disk (creating the file, and any
    /// nested sections the key's ":" segments imply, if they do not exist yet) without touching any
    /// other key already in the file, and reloads <paramref name="configuration"/> so the new value
    /// is visible immediately — no restart needed. Shared by first-run secret generation and by the
    /// Admin/API sync-secret bootstrap handshake.
    /// </summary>
    public static bool TrySetLocalSetting(
        IConfiguration configuration,
        IHostEnvironment environment,
        string key,
        string value,
        out string? error)
    {
        var path = Path.Combine(environment.ContentRootPath, LocalSettingsFileName);

        try
        {
            JsonObject root;
            if (File.Exists(path))
            {
                var existing = File.ReadAllText(path);
                root = string.IsNullOrWhiteSpace(existing)
                    ? new JsonObject()
                    : (JsonNode.Parse(existing) as JsonObject ?? new JsonObject());
            }
            else
            {
                root = new JsonObject();
            }

            var segments = key.Split(':');
            var node = root;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                if (node[segments[i]] is not JsonObject child)
                {
                    child = new JsonObject();
                    node[segments[i]] = child;
                }
                node = child;
            }

            node[segments[^1]] = value;

            File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

            // Picks up the file we just wrote (localsettings.json is already a registered source);
            // without this the freshly written value would not appear until the next process start.
            if (configuration is IConfigurationRoot configRoot) configRoot.Reload();

            error = null;
            return true;
        }
        catch (Exception ex)
        {
            error = $"Could not save '{key}': failed to write '{path}' ({ex.Message}).";
            return false;
        }
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
