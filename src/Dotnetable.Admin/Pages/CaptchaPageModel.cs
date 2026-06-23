using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetable.Admin.Pages;

/// <summary>
/// Base page model that wires the shared bot protection (Cloudflare Turnstile with a math-captcha
/// fallback) into the public admin forms. Pages call <see cref="PrepareCaptcha"/> on GET and
/// <see cref="ValidateCaptchaAsync"/> on POST before performing their action.
/// </summary>
public abstract class CaptchaPageModel : PageModel, ICaptchaView
{
    protected readonly IHumanVerificationService Human;

    protected CaptchaPageModel(IHumanVerificationService human) => Human = human;

    public bool CaptchaUseTurnstile { get; private set; }
    public string? TurnstileSiteKey { get; private set; }
    public string CaptchaSvg { get; private set; } = string.Empty;
    public string? CaptchaError { get; set; }

    [BindProperty] public string? MathToken { get; set; }
    [BindProperty] public string? MathAnswer { get; set; }

    /// <summary>Builds a fresh challenge for rendering. A math challenge is always created so the
    /// Turnstile fallback (and math-only mode) has one ready.</summary>
    protected void PrepareCaptcha(bool forceMath = false)
    {
        CaptchaUseTurnstile = Human.TurnstileEnabled && !forceMath;
        TurnstileSiteKey = Human.TurnstileSiteKey;

        var challenge = Human.CreateMathChallenge();
        MathToken = challenge.Token;
        CaptchaSvg = challenge.Svg;
        MathAnswer = null;
    }

    /// <summary>
    /// Verifies the human challenge. On failure, re-prepares the captcha for redisplay and sets
    /// <see cref="CaptchaError"/>. Returns true only when verification succeeds.
    /// </summary>
    protected async Task<bool> ValidateCaptchaAsync(CancellationToken ct = default)
    {
        var turnstileToken = Request.Form["cf-turnstile-response"].ToString();

        if (Human.TurnstileEnabled && !string.IsNullOrEmpty(turnstileToken))
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await Human.VerifyTurnstileAsync(turnstileToken, ip, ct);

            if (result == true) return true;
            if (result == false)
            {
                CaptchaError = "Verification failed. Please try again.";
                PrepareCaptcha();
                return false;
            }

            // result == null → Turnstile unreachable. Fall back to the math captcha.
            if (Human.ValidateMath(MathToken, MathAnswer)) return true;
            CaptchaError = "Verification service is unavailable. Please solve the captcha below.";
            PrepareCaptcha(forceMath: true);
            return false;
        }

        // No Turnstile token: either Turnstile is disabled, or the user used the JS math fallback.
        if (Human.TurnstileEnabled && string.IsNullOrWhiteSpace(MathAnswer))
        {
            CaptchaError = "Please complete the verification.";
            PrepareCaptcha();
            return false;
        }

        if (Human.ValidateMath(MathToken, MathAnswer)) return true;

        CaptchaError = "Incorrect captcha answer.";
        PrepareCaptcha(forceMath: Human.TurnstileEnabled);
        return false;
    }
}
