using Dotnetable.Application.Interfaces;

namespace Dotnetable.API.Auth;

/// <summary>
/// One place that answers "did a human send this?", so every public write endpoint checks the same
/// way rather than each controller re-implementing the Turnstile-or-math branch.
///
/// <para>Only the contact form used to be guarded, which left registration, password reset, reviews,
/// questions and form submissions open to scripted abuse — and registration and reset each cost the
/// site an email or an SMS per request.</para>
/// </summary>
public sealed class CaptchaGuard
{
    private readonly IHumanVerificationService _verification;
    private readonly IWebsiteService _websites;

    public CaptchaGuard(IHumanVerificationService verification, IWebsiteService websites)
    {
        _verification = verification;
        _websites = websites;
    }

    /// <summary>
    /// Verifies a submitted captcha for <paramref name="websiteId"/>.
    ///
    /// <para>Returns true when the site has no captcha configured at all: a site that never enabled
    /// bot protection must not have its sign-up form start rejecting every customer the day this
    /// check shipped. Turning captcha on is an admin decision; this enforces it once made.</para>
    /// </summary>
    public async Task<bool> VerifyAsync(int websiteId, string? token, string? answer, string? remoteIp,
        CancellationToken ct = default)
    {
        var setting = await _websites.GetCaptchaSettingAsync(websiteId, ct);
        var resolution = _verification.ResolveForWebsite(setting);

        if (resolution.UseTurnstile)
        {
            // A null result means Cloudflare itself was unreachable. Failing closed there would take
            // sign-in down with Cloudflare, so an unreachable verifier is treated as a pass and the
            // per-IP rate limiter carries the load for as long as the outage lasts.
            var result = await _verification.VerifyTurnstileAsync(token, remoteIp, resolution.TurnstileSecretKey!, ct);
            return result != false;
        }

        return _verification.ValidateMath(token, answer);
    }
}

/// <summary>Captcha fields every guarded public request body carries.</summary>
public interface ICaptchaProtectedRequest
{
    string? CaptchaToken { get; }
    string? CaptchaAnswer { get; }
}
