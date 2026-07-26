using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Bot protection for the public admin forms. Prefers Cloudflare Turnstile when it is configured
/// and selected; otherwise (or when Turnstile is unreachable) uses a self-contained image captcha
/// that asks the user to add or subtract two small numbers. The expected answer is kept server-side
/// in a short-lived <see cref="IMemoryCache"/> entry. The challenge is drawn as noisy path strokes
/// (no SVG <c>&lt;text&gt;</c>), so the equation is not trivially scrapable from the markup.
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
        // Only add/subtract; operands are 1..29 so two-digit numbers stay modest.
        int a = RandomNumberGenerator.GetInt32(1, 30);
        int b = RandomNumberGenerator.GetInt32(1, 30);
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

    public Task<bool?> VerifyTurnstileAsync(string? token, string? remoteIp, CancellationToken ct = default) =>
        VerifyTurnstileAsync(token, remoteIp, _settings.Security.TurnstileSecretKey, ct);

    public CaptchaResolution ResolveForWebsite(WebsiteCaptchaSetting? setting)
    {
        if (setting is { Provider: (byte)CaptchaProvider.Turnstile } &&
            !string.IsNullOrWhiteSpace(setting.TurnstileSiteKey) &&
            !string.IsNullOrWhiteSpace(setting.TurnstileSecretKey))
        {
            return new CaptchaResolution
            {
                UseTurnstile = true,
                TurnstileSiteKey = setting.TurnstileSiteKey,
                TurnstileSecretKey = setting.TurnstileSecretKey,
            };
        }

        return new CaptchaResolution { UseTurnstile = false };
    }

    public async Task<bool?> VerifyTurnstileAsync(string? token, string? remoteIp, string secretKey, CancellationToken ct = default)
    {
        var secret = secretKey;
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

    /// <summary>
    /// Renders the math question as path-based glyphs (no extractable <c>&lt;text&gt;</c> nodes) with
    /// heavy visual noise so scrapers cannot read the challenge from the SVG DOM and simple OCR has
    /// a harder time. Digits remain human-readable.
    /// </summary>
    private static string RenderSvg(string text)
    {
        // Wide enough for two-digit operands, e.g. "29 − 18 = ?".
        const int width = 200;
        const int height = 58;
        int Jitter(int max) => RandomNumberGenerator.GetInt32(-max, max + 1);
        string RandColor(int min, int max) =>
            $"#{RandomNumberGenerator.GetInt32(min, max):x2}{RandomNumberGenerator.GetInt32(min, max):x2}{RandomNumberGenerator.GetInt32(min, max):x2}";

        var sb = new StringBuilder(2048);
        sb.Append($"<svg xmlns='http://www.w3.org/2000/svg' width='{width}' height='{height}' viewBox='0 0 {width} {height}' role='img' aria-label='captcha'>");
        sb.Append($"<defs><clipPath id='c'><rect width='{width}' height='{height}' rx='8'/></clipPath></defs>");
        sb.Append($"<rect width='{width}' height='{height}' rx='8' fill='{RandColor(0xe8, 0xf4)}'/>");
        sb.Append("<g clip-path='url(#c)'>");

        // Speckle dots (background clutter).
        for (int i = 0; i < 55; i++)
        {
            int cx = RandomNumberGenerator.GetInt32(0, width);
            int cy = RandomNumberGenerator.GetInt32(0, height);
            double r = 0.4 + RandomNumberGenerator.GetInt32(0, 12) / 10.0;
            sb.Append(CultureInfo.InvariantCulture,
                $"<circle cx='{cx}' cy='{cy}' r='{r:0.#}' fill='{RandColor(0x90, 0xd0)}' opacity='0.55'/>");
        }

        // Curved noise strokes behind the glyphs.
        for (int i = 0; i < 6; i++)
        {
            int x1 = RandomNumberGenerator.GetInt32(0, width);
            int y1 = RandomNumberGenerator.GetInt32(0, height);
            int cx1 = RandomNumberGenerator.GetInt32(0, width);
            int cy1 = RandomNumberGenerator.GetInt32(0, height);
            int cx2 = RandomNumberGenerator.GetInt32(0, width);
            int cy2 = RandomNumberGenerator.GetInt32(0, height);
            int x2 = RandomNumberGenerator.GetInt32(0, width);
            int y2 = RandomNumberGenerator.GetInt32(0, height);
            double sw = 0.6 + RandomNumberGenerator.GetInt32(0, 16) / 10.0;
            sb.Append(CultureInfo.InvariantCulture,
                $"<path d='M{x1} {y1} C{cx1} {cy1} {cx2} {cy2} {x2} {y2}' fill='none' stroke='{RandColor(0xa0, 0xd8)}' stroke-width='{sw:0.#}' opacity='0.7'/>");
        }

        // Decoy path fragments that look a bit like strokes (not real digits).
        for (int i = 0; i < 8; i++)
        {
            int x = RandomNumberGenerator.GetInt32(4, width - 12);
            int y = RandomNumberGenerator.GetInt32(6, height - 10);
            int dx = Jitter(10);
            int dy = Jitter(10);
            sb.Append($"<path d='M{x} {y} l{dx} {dy} l{Jitter(8)} {Jitter(8)}' fill='none' stroke='{RandColor(0xb0, 0xd8)}' stroke-width='1.2' opacity='0.45'/>");
        }

        // Real glyphs drawn as stroked polylines — never as SVG <text>, so the answer is not in the markup.
        double cursorX = 10 + Jitter(3);
        const double baseY = 10;
        const double glyphW = 14;
        const double glyphH = 28;
        const double advance = 18.5;

        foreach (var ch in text)
        {
            if (ch == ' ')
            {
                cursorX += advance * 0.45;
                continue;
            }

            if (!GlyphStrokes.TryGetValue(ch, out var strokes))
            {
                cursorX += advance * 0.5;
                continue;
            }

            double scale = 0.92 + RandomNumberGenerator.GetInt32(0, 20) / 100.0;
            double rot = Jitter(18);
            double ox = cursorX + Jitter(2);
            double oy = baseY + Jitter(5);
            string ink = RandColor(0x28, 0x52);
            double strokeW = 2.1 + RandomNumberGenerator.GetInt32(0, 10) / 10.0;

            sb.Append(CultureInfo.InvariantCulture,
                $"<g transform='translate({ox:0.##} {oy:0.##}) rotate({rot}) scale({scale:0.###})'>");

            foreach (var stroke in strokes)
            {
                if (stroke.Length < 2) continue;
                var d = new StringBuilder();
                for (int i = 0; i < stroke.Length; i++)
                {
                    // Mild per-point warp so the same digit never looks identical twice.
                    double px = stroke[i].X * glyphW + Jitter(1) * 0.35;
                    double py = stroke[i].Y * glyphH + Jitter(1) * 0.35;
                    d.Append(i == 0 ? 'M' : 'L');
                    d.Append(CultureInfo.InvariantCulture, $"{px:0.##} {py:0.##} ");
                }
                sb.Append(CultureInfo.InvariantCulture,
                    $"<path d='{d}' fill='none' stroke='{ink}' stroke-width='{strokeW:0.#}' stroke-linecap='round' stroke-linejoin='round'/>");
            }

            sb.Append("</g>");
            cursorX += advance + Jitter(1) * 0.4;
        }

        // Crossing lines over the text (classic captcha interference).
        for (int i = 0; i < 5; i++)
        {
            int x1 = RandomNumberGenerator.GetInt32(0, width / 3);
            int y1 = RandomNumberGenerator.GetInt32(8, height - 8);
            int x2 = RandomNumberGenerator.GetInt32(width * 2 / 3, width);
            int y2 = RandomNumberGenerator.GetInt32(8, height - 8);
            int mx = (x1 + x2) / 2 + Jitter(20);
            int my = (y1 + y2) / 2 + Jitter(12);
            double sw = 0.8 + RandomNumberGenerator.GetInt32(0, 12) / 10.0;
            sb.Append(CultureInfo.InvariantCulture,
                $"<path d='M{x1} {y1} Q{mx} {my} {x2} {y2}' fill='none' stroke='{RandColor(0x70, 0xb0)}' stroke-width='{sw:0.#}' opacity='0.75'/>");
        }

        // Foreground speckles on top of glyphs.
        for (int i = 0; i < 25; i++)
        {
            int cx = RandomNumberGenerator.GetInt32(0, width);
            int cy = RandomNumberGenerator.GetInt32(0, height);
            double r = 0.3 + RandomNumberGenerator.GetInt32(0, 10) / 10.0;
            sb.Append(CultureInfo.InvariantCulture,
                $"<circle cx='{cx}' cy='{cy}' r='{r:0.#}' fill='{RandColor(0x50, 0xa0)}' opacity='0.4'/>");
        }

        sb.Append("</g></svg>");
        return sb.ToString();
    }

    /// <summary>
    /// Normalized (0..1) polylines for each captcha glyph. Drawn as strokes so markup never contains
    /// the digit characters themselves.
    /// </summary>
    private static readonly Dictionary<char, (double X, double Y)[][]> GlyphStrokes = new()
    {
        ['0'] =
        [
            [(0.15, 0.08), (0.85, 0.08), (0.95, 0.22), (0.95, 0.78), (0.85, 0.92), (0.15, 0.92), (0.05, 0.78), (0.05, 0.22), (0.15, 0.08)],
        ],
        ['1'] =
        [
            [(0.35, 0.18), (0.55, 0.08), (0.55, 0.92)],
            [(0.28, 0.92), (0.78, 0.92)],
        ],
        ['2'] =
        [
            [(0.12, 0.28), (0.18, 0.12), (0.55, 0.05), (0.88, 0.18), (0.88, 0.35), (0.15, 0.72), (0.12, 0.92), (0.90, 0.92)],
        ],
        ['3'] =
        [
            [(0.15, 0.12), (0.80, 0.12), (0.90, 0.28), (0.55, 0.48), (0.88, 0.62), (0.88, 0.80), (0.70, 0.94), (0.15, 0.90)],
            [(0.40, 0.48), (0.55, 0.48)],
        ],
        ['4'] =
        [
            [(0.72, 0.92), (0.72, 0.08), (0.12, 0.62), (0.92, 0.62)],
        ],
        ['5'] =
        [
            [(0.85, 0.08), (0.18, 0.08), (0.12, 0.48), (0.55, 0.42), (0.88, 0.55), (0.88, 0.78), (0.65, 0.94), (0.18, 0.90)],
        ],
        ['6'] =
        [
            [(0.78, 0.18), (0.55, 0.06), (0.22, 0.15), (0.10, 0.45), (0.12, 0.78), (0.32, 0.94), (0.72, 0.90), (0.90, 0.70), (0.85, 0.52), (0.55, 0.42), (0.18, 0.50)],
        ],
        ['7'] =
        [
            [(0.12, 0.10), (0.90, 0.10), (0.42, 0.92)],
            [(0.30, 0.48), (0.72, 0.48)],
        ],
        ['8'] =
        [
            [(0.50, 0.48), (0.78, 0.38), (0.82, 0.18), (0.55, 0.06), (0.22, 0.14), (0.18, 0.34), (0.50, 0.48), (0.82, 0.60), (0.82, 0.80), (0.55, 0.94), (0.18, 0.84), (0.15, 0.64), (0.50, 0.48)],
        ],
        ['9'] =
        [
            [(0.22, 0.82), (0.45, 0.94), (0.78, 0.85), (0.90, 0.55), (0.88, 0.22), (0.62, 0.06), (0.22, 0.12), (0.10, 0.32), (0.18, 0.50), (0.55, 0.58), (0.88, 0.48)],
        ],
        ['+'] =
        [
            [(0.50, 0.18), (0.50, 0.82)],
            [(0.18, 0.50), (0.82, 0.50)],
        ],
        ['−'] =
        [
            [(0.18, 0.50), (0.82, 0.50)],
        ],
        ['='] =
        [
            [(0.15, 0.38), (0.85, 0.38)],
            [(0.15, 0.62), (0.85, 0.62)],
        ],
        ['?'] =
        [
            [(0.22, 0.28), (0.28, 0.12), (0.55, 0.06), (0.82, 0.18), (0.78, 0.38), (0.50, 0.50), (0.50, 0.68)],
            [(0.50, 0.82), (0.50, 0.90)],
        ],
    };

    private sealed class TurnstileResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
    }
}
