using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Bot protection for the public admin forms. Prefers Cloudflare Turnstile when it is configured
/// and selected; otherwise (or when Turnstile is unreachable) uses a self-contained image captcha
/// that asks the user to add or subtract two small numbers. The expected answer is kept server-side
/// in a short-lived <see cref="IMemoryCache"/> entry, so the rendered SVG never reveals it.
/// </summary>
public class HumanVerificationService : IHumanVerificationService
{
    private const string TurnstileVerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
    private static readonly TimeSpan ChallengeLifetime = TimeSpan.FromMinutes(5);

    // SmtpClient-style single shared client; Turnstile verification is low volume.
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };

    private readonly IAppSettingsStore _settings;
    private readonly IMemoryCache _cache;

    public HumanVerificationService(IAppSettingsStore settings, IMemoryCache cache)
    {
        _settings = settings;
        _cache = cache;
    }

    public bool TurnstileEnabled => _settings.Security.UseTurnstile;

    public string? TurnstileSiteKey =>
        TurnstileEnabled ? _settings.Security.TurnstileSiteKey : null;

    public MathChallenge CreateMathChallenge()
    {
        int a = RandomNumberGenerator.GetInt32(1, 10);
        int b = RandomNumberGenerator.GetInt32(1, 10);
        bool subtract = RandomNumberGenerator.GetInt32(0, 2) == 1;

        // Keep subtraction non-negative for a friendlier puzzle.
        if (subtract && b > a) (a, b) = (b, a);

        int answer = subtract ? a - b : a + b;
        char op = subtract ? '−' : '+';

        var token = Guid.NewGuid().ToString("N");
        _cache.Set(CacheKey(token), answer, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ChallengeLifetime,
        });

        return new MathChallenge { Token = token, Svg = RenderSvg($"{a} {op} {b} = ?") };
    }

    public bool ValidateMath(string? token, string? answer)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(answer)) return false;
        if (!_cache.TryGetValue(CacheKey(token), out int expected)) return false;

        // Consume on first use so a captcha can't be replayed.
        _cache.Remove(CacheKey(token));

        return int.TryParse(answer.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var given)
               && given == expected;
    }

    public async Task<bool?> VerifyTurnstileAsync(string? token, string? remoteIp, CancellationToken ct = default)
    {
        var secret = _settings.Security.TurnstileSecretKey;
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var form = new List<KeyValuePair<string, string>>
            {
                new("secret", secret),
                new("response", token),
            };
            if (!string.IsNullOrWhiteSpace(remoteIp))
                form.Add(new KeyValuePair<string, string>("remoteip", remoteIp));

            using var content = new FormUrlEncodedContent(form);
            using var response = await Http.PostAsync(TurnstileVerifyUrl, content, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TurnstileResponse>(cancellationToken: ct);
            return result?.Success ?? false;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Cloudflare unreachable / timed out — signal "unavailable" so callers can fall back.
            return null;
        }
    }

    private static string CacheKey(string token) => $"mathcaptcha:{token}";

    /// <summary>Renders the question as a small, slightly noisy inline SVG.</summary>
    private static string RenderSvg(string text)
    {
        // Deterministic-ish jitter per call keeps every captcha visually distinct.
        int Jitter(int max) => RandomNumberGenerator.GetInt32(-max, max + 1);

        var sb = new StringBuilder();
        sb.Append("<svg xmlns='http://www.w3.org/2000/svg' width='150' height='52' viewBox='0 0 150 52' role='img' aria-label='captcha'>");
        sb.Append("<rect width='150' height='52' rx='8' fill='#eef1f6'/>");

        // Noise lines.
        for (int i = 0; i < 4; i++)
        {
            int x1 = RandomNumberGenerator.GetInt32(0, 150), y1 = RandomNumberGenerator.GetInt32(0, 52);
            int x2 = RandomNumberGenerator.GetInt32(0, 150), y2 = RandomNumberGenerator.GetInt32(0, 52);
            sb.Append($"<line x1='{x1}' y1='{y1}' x2='{x2}' y2='{y2}' stroke='#c2cad6' stroke-width='1'/>");
        }

        // Each glyph drawn separately with a little rotation/offset.
        int x = 16;
        foreach (var ch in text)
        {
            if (ch != ' ')
            {
                int dy = Jitter(4);
                int rot = Jitter(12);
                var safe = System.Net.WebUtility.HtmlEncode(ch.ToString());
                sb.Append($"<text x='{x}' y='{34 + dy}' font-family='Consolas,monospace' font-size='26' font-weight='700' fill='#37415a' transform='rotate({rot} {x} 30)'>{safe}</text>");
            }
            x += 13;
        }

        sb.Append("</svg>");
        return sb.ToString();
    }

    private sealed class TurnstileResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
    }
}
