using System.ComponentModel.DataAnnotations;
using Dotnetable.API.Auth;
using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Authentication for website customers (<see cref="WebsiteClient"/>). Customers self-register with an
/// email or mobile number, activate with a one-time code, then sign in for a JWT bearer token plus a
/// rotating refresh token. The target website is resolved from the <c>X-Website-Key</c> header (the
/// site's <see cref="Website.AuthCode"/>).
///
/// <para>Every endpoint here is rate limited per IP and every code-sending endpoint is captcha
/// guarded, because each of them either checks a credential or spends money on delivery.</para>
/// </summary>
[EnableRateLimiting(RateLimiting.AuthPolicy)]
public class AuthController : BaseController
{
    /// <summary>Header carrying the caller website's per-site key (<see cref="Website.AuthCode"/>).</summary>
    public const string WebsiteKeyHeader = "X-Website-Key";

    private readonly IWebsiteClientAuthService _auth;
    private readonly IJwtTokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly ILoginLogService _loginLog;
    private readonly IWebsiteService _websiteService;
    private readonly CaptchaGuard _captcha;

    public AuthController(
        IWebsiteClientAuthService auth,
        IJwtTokenService tokenService,
        IRefreshTokenService refreshTokens,
        ILoginLogService loginLog,
        IWebsiteService websiteService,
        CaptchaGuard captcha)
    {
        _auth = auth;
        _tokenService = tokenService;
        _refreshTokens = refreshTokens;
        _loginLog = loginLog;
        _websiteService = websiteService;
        _captcha = captcha;
    }

    // ── Request bodies ──────────────────────────────────────────────

    public sealed class RegisterRequest : ICaptchaProtectedRequest
    {
        public string? GivenName { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? CountryCode { get; set; }
        public string? Cellphone { get; set; }

        // Length is bounded here so an oversized value never reaches the hasher; the real strength
        // rules live in PasswordPolicy so the panel and the storefront cannot drift apart.
        [Required, StringLength(256, MinimumLength = 10)]
        public string Password { get; set; } = string.Empty;

        public string? CaptchaToken { get; set; }
        public string? CaptchaAnswer { get; set; }
    }

    public sealed class IdentifierRequest : ICaptchaProtectedRequest
    {
        /// <summary>Email address or mobile number the customer registered with.</summary>
        [Required, StringLength(120)] public string Identifier { get; set; } = string.Empty;

        public string? CaptchaToken { get; set; }
        public string? CaptchaAnswer { get; set; }
    }

    public sealed class VerifyRequest
    {
        [Required, StringLength(120)] public string Identifier { get; set; } = string.Empty;
        [Required, StringLength(16)] public string Code { get; set; } = string.Empty;
    }

    public sealed class LoginRequest : ICaptchaProtectedRequest
    {
        [Required, StringLength(120)] public string Identifier { get; set; } = string.Empty;
        [Required, StringLength(256)] public string Password { get; set; } = string.Empty;

        public string? CaptchaToken { get; set; }
        public string? CaptchaAnswer { get; set; }
    }

    public sealed class ResetRequest
    {
        [Required, StringLength(120)] public string Identifier { get; set; } = string.Empty;
        [Required, StringLength(16)] public string Code { get; set; } = string.Empty;
        [Required, StringLength(256, MinimumLength = 10)] public string NewPassword { get; set; } = string.Empty;
    }

    public sealed class RefreshRequest
    {
        [Required, StringLength(200)] public string RefreshToken { get; set; } = string.Empty;
    }

    // ── Endpoints ───────────────────────────────────────────────────

    /// <summary>Creates an inactive account and sends an activation code. Captcha guarded and throttled.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimiting.OtpPolicy)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct = default)
    {
        if (await ResolveWebsiteAsync(ct) is not { } website)
            return BadRequest(new { message = "Unknown or missing website." });

        if (!await VerifyCaptchaAsync(website.WebsiteID, request, ct))
            return BadRequest(new { message = "Captcha verification failed. Please try again." });

        var response = await _auth.RegisterAsync(new ClientRegistration(
            website.WebsiteID, request.GivenName, request.Surname,
            request.Email, request.CountryCode, request.Cellphone, request.Password), ct);

        return response.Result switch
        {
            ClientRegisterResult.OtpSent => Ok(new
            {
                success = true,
                channel = response.Channel.ToString(),
                identifier = response.Identifier,
                message = response.Channel == OtpChannel.Email
                    ? "We sent a verification code to your email."
                    : "We sent a verification code to your mobile.",
            }),
            ClientRegisterResult.AlreadyRegistered => Conflict(new
            {
                message = "An account with this email or mobile already exists. Please sign in.",
            }),
            ClientRegisterResult.WeakPassword => BadRequest(new
            {
                message = response.Error ?? "Please choose a stronger password.",
            }),
            ClientRegisterResult.DeliveryNotConfigured => StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = response.Channel == OtpChannel.Email
                    ? "Verification email cannot be sent — email is not configured for this website."
                    : "Verification SMS cannot be sent — no SMS gateway is configured for this website.",
            }),
            _ => BadRequest(new { message = "Please provide a valid email or mobile number and a password." }),
        };
    }

    [HttpPost("verify-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyRequest request, CancellationToken ct = default)
    {
        if (await ResolveWebsiteAsync(ct) is not { } website)
            return BadRequest(new { message = "Unknown or missing website." });

        var (result, client) = await _auth.VerifyOtpAsync(website.WebsiteID, request.Identifier, request.Code, ct);
        if (result == ClientVerifyResult.Success && client is not null)
            return Ok(await IssueSessionAsync(client, ct));

        return result switch
        {
            ClientVerifyResult.AlreadyActive => Ok(new { success = true, message = "Account already activated. Please sign in." }),
            ClientVerifyResult.NotFound => NotFound(new { message = "No pending registration found for this account." }),
            ClientVerifyResult.TooManyAttempts => StatusCode(StatusCodes.Status429TooManyRequests, new
            {
                message = "Too many incorrect codes. Request a new code and try again.",
            }),
            _ => BadRequest(new { message = "The code is invalid or has expired." }),
        };
    }

    [HttpPost("resend-otp")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimiting.OtpPolicy)]
    public async Task<IActionResult> ResendOtp([FromBody] IdentifierRequest request, CancellationToken ct = default)
    {
        if (await ResolveWebsiteAsync(ct) is not { } website)
            return BadRequest(new { message = "Unknown or missing website." });

        if (!await VerifyCaptchaAsync(website.WebsiteID, request, ct))
            return BadRequest(new { message = "Captcha verification failed. Please try again." });

        var result = await _auth.ResendOtpAsync(website.WebsiteID, request.Identifier, ct);
        return result switch
        {
            ClientResendResult.OtpSent => Ok(new { success = true, message = "A new code has been sent." }),
            ClientResendResult.AlreadyActive => Ok(new { success = true, message = "Account already activated. Please sign in." }),
            ClientResendResult.NotFound => NotFound(new { message = "No pending registration found for this account." }),
            ClientResendResult.TooSoon => StatusCode(StatusCodes.Status429TooManyRequests, new
            {
                message = "A code was just sent. Please wait a minute before requesting another.",
            }),
            _ => StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Code cannot be sent right now." }),
        };
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct = default)
    {
        if (await ResolveWebsiteAsync(ct) is not { } website)
            return BadRequest(new { message = "Unknown or missing website." });

        if (!await VerifyCaptchaAsync(website.WebsiteID, request, ct))
            return BadRequest(new { message = "Captcha verification failed. Please try again." });

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var (status, client) = await _auth.ValidateCredentialsAsync(
            website.WebsiteID, request.Identifier, request.Password, ct);

        await _loginLog.RecordAsync(request.Identifier, website.WebsiteID, status == ClientLoginStatus.Success, ip, ct);

        return status switch
        {
            ClientLoginStatus.Success when client is not null => Ok(await IssueSessionAsync(client, ct)),
            ClientLoginStatus.NotActivated => StatusCode(StatusCodes.Status403Forbidden, new
            {
                status = "NotActivated",
                identifier = request.Identifier,
                message = "Your account is not activated yet. Please enter the code we sent you.",
            }),
            ClientLoginStatus.LockedOut => StatusCode(StatusCodes.Status423Locked, new
            {
                status = "LockedOut",
                message = "Too many failed sign-in attempts. Please try again in a few minutes.",
            }),
            _ => Unauthorized(new { message = "Invalid credentials." }),
        };
    }

    /// <summary>
    /// Exchanges a refresh token for a new access/refresh pair, so a customer is not signed out every
    /// time the short-lived access token expires. Single-use: the presented token is revoked here.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct = default)
    {
        if (await ResolveWebsiteAsync(ct) is not { } website)
            return BadRequest(new { message = "Unknown or missing website." });

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _refreshTokens.ExchangeAsync(website.WebsiteID, request.RefreshToken, ip, ct);

        if (!result.Success)
        {
            // A replayed token has already invalidated the family; say the same thing either way so
            // the response cannot be used to probe which tokens once existed.
            return Unauthorized(new { message = "Your session has expired. Please sign in again." });
        }

        var token = _tokenService.CreateToken(result.Client!);
        return Ok(new
        {
            accessToken = token.AccessToken,
            expiresAtUtc = token.ExpiresAtUtc,
            tokenType = "Bearer",
            refreshToken = result.Refresh!.Token,
            refreshExpiresAtUtc = result.Refresh.ExpiresAtUtc,
        });
    }

    /// <summary>Revokes the presented refresh token (sign-out on this device).</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request, CancellationToken ct = default)
    {
        await _refreshTokens.RevokeAsync(request.RefreshToken, ct);
        return Ok(new { success = true });
    }

    /// <summary>Revokes every refresh token for the signed-in customer (sign out everywhere).</summary>
    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll(CancellationToken ct = default)
    {
        if (int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var clientId))
            await _refreshTokens.RevokeAllForClientAsync(clientId, ct);

        return Ok(new { success = true });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimiting.OtpPolicy)]
    public async Task<IActionResult> ForgotPassword([FromBody] IdentifierRequest request, CancellationToken ct = default)
    {
        if (await ResolveWebsiteAsync(ct) is not { } website)
            return BadRequest(new { message = "Unknown or missing website." });

        if (!await VerifyCaptchaAsync(website.WebsiteID, request, ct))
            return BadRequest(new { message = "Captcha verification failed. Please try again." });

        var result = await _auth.RequestPasswordResetAsync(website.WebsiteID, request.Identifier, ct);
        // Don't reveal whether the account exists; report a generic success unless delivery is impossible.
        return result switch
        {
            ClientResetRequestResult.DeliveryNotConfigured => StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Reset code cannot be sent — no delivery channel is configured for this website.",
            }),
            _ => Ok(new { success = true, message = "If the account exists, a reset code has been sent." }),
        };
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetRequest request, CancellationToken ct = default)
    {
        if (await ResolveWebsiteAsync(ct) is not { } website)
            return BadRequest(new { message = "Unknown or missing website." });

        var result = await _auth.ResetPasswordAsync(
            website.WebsiteID, request.Identifier, request.Code, request.NewPassword, ct);

        return result switch
        {
            ClientResetResult.Success => Ok(new { success = true, message = "Your password has been reset. You can sign in now." }),
            ClientResetResult.NotFound => NotFound(new { message = "No account found for this email or mobile." }),
            ClientResetResult.WeakPassword => BadRequest(new
            {
                message = "Please choose a stronger password: at least 10 characters, mixing upper case, lower case, digits or symbols.",
            }),
            ClientResetResult.TooManyAttempts => StatusCode(StatusCodes.Status429TooManyRequests, new
            {
                message = "Too many incorrect codes. Request a new code and try again.",
            }),
            _ => BadRequest(new { message = "The code is invalid or has expired." }),
        };
    }

    /// <summary>Returns the identity and level of the bearer-token caller.</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new
    {
        clientId = User.FindFirst(ClientClaims.ClientId)?.Value,
        websiteId = User.FindFirst(MemberClaims.WebsiteId)?.Value,
        level = User.FindFirst(ClientClaims.ClientLevel)?.Value,
        name = User.Identity?.Name,
    });

    // ── Helpers ─────────────────────────────────────────────────────

    /// <summary>Issues the access + refresh pair that represents a signed-in customer session.</summary>
    private async Task<object> IssueSessionAsync(WebsiteClient client, CancellationToken ct)
    {
        var token = _tokenService.CreateToken(client);
        var refresh = await _refreshTokens.IssueAsync(
            client, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);

        return new
        {
            accessToken = token.AccessToken,
            expiresAtUtc = token.ExpiresAtUtc,
            tokenType = "Bearer",
            refreshToken = refresh.Token,
            refreshExpiresAtUtc = refresh.ExpiresAtUtc,
        };
    }

    private Task<bool> VerifyCaptchaAsync(int websiteId, ICaptchaProtectedRequest request, CancellationToken ct) =>
        _captcha.VerifyAsync(websiteId, request.CaptchaToken, request.CaptchaAnswer,
            HttpContext.Connection.RemoteIpAddress?.ToString(), ct);

    private async Task<Website?> ResolveWebsiteAsync(CancellationToken ct)
    {
        if (!Request.Headers.TryGetValue(WebsiteKeyHeader, out var keyValue) ||
            !Guid.TryParse(keyValue.ToString(), out var authCode))
            return null;

        var website = await _websiteService.GetByAuthCodeAsync(authCode, ct);
        return website is { Active: true } ? website : null;
    }
}
