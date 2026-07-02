using System.ComponentModel.DataAnnotations;
using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Authentication for website customers (<see cref="WebsiteClient"/>). Customers self-register with an
/// email or mobile number, activate with a one-time code, then sign in for a JWT bearer token. The
/// target website is resolved from the <c>X-Website-Key</c> header (the site's <see cref="Website.AuthCode"/>).
/// </summary>
public class AuthController : BaseController
{
    /// <summary>Header carrying the caller website's per-site key (<see cref="Website.AuthCode"/>).</summary>
    public const string WebsiteKeyHeader = "X-Website-Key";

    private readonly IWebsiteClientAuthService _auth;
    private readonly IJwtTokenService _tokenService;
    private readonly ILoginLogService _loginLog;
    private readonly IWebsiteService _websiteService;

    public AuthController(
        IWebsiteClientAuthService auth,
        IJwtTokenService tokenService,
        ILoginLogService loginLog,
        IWebsiteService websiteService)
    {
        _auth = auth;
        _tokenService = tokenService;
        _loginLog = loginLog;
        _websiteService = websiteService;
    }

    // ── Request bodies ──────────────────────────────────────────────

    public sealed class RegisterRequest
    {
        public string? GivenName { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? CountryCode { get; set; }
        public string? Cellphone { get; set; }
        [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
    }

    public sealed class IdentifierRequest
    {
        /// <summary>Email address or mobile number the customer registered with.</summary>
        [Required] public string Identifier { get; set; } = string.Empty;
    }

    public sealed class VerifyRequest
    {
        [Required] public string Identifier { get; set; } = string.Empty;
        [Required] public string Code { get; set; } = string.Empty;
    }

    public sealed class LoginRequest
    {
        [Required] public string Identifier { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }

    public sealed class ResetRequest
    {
        [Required] public string Identifier { get; set; } = string.Empty;
        [Required] public string Code { get; set; } = string.Empty;
        [Required, MinLength(6)] public string NewPassword { get; set; } = string.Empty;
    }

    // ── Endpoints ───────────────────────────────────────────────────

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct = default)
    {
        if (await ResolveWebsiteAsync(ct) is not { } website)
            return BadRequest(new { message = "Unknown or missing website." });

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
            ClientRegisterResult.DeliveryNotConfigured => StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Verification email cannot be sent — email is not configured for this website.",
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
            return Ok(IssueToken(client));

        return result switch
        {
            ClientVerifyResult.AlreadyActive => Ok(new { success = true, message = "Account already activated. Please sign in." }),
            ClientVerifyResult.NotFound => NotFound(new { message = "No pending registration found for this account." }),
            _ => BadRequest(new { message = "The code is invalid or has expired." }),
        };
    }

    [HttpPost("resend-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendOtp([FromBody] IdentifierRequest request, CancellationToken ct = default)
    {
        if (await ResolveWebsiteAsync(ct) is not { } website)
            return BadRequest(new { message = "Unknown or missing website." });

        var result = await _auth.ResendOtpAsync(website.WebsiteID, request.Identifier, ct);
        return result switch
        {
            ClientResendResult.OtpSent => Ok(new { success = true, message = "A new code has been sent." }),
            ClientResendResult.AlreadyActive => Ok(new { success = true, message = "Account already activated. Please sign in." }),
            ClientResendResult.NotFound => NotFound(new { message = "No pending registration found for this account." }),
            _ => StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Code cannot be sent right now." }),
        };
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct = default)
    {
        if (await ResolveWebsiteAsync(ct) is not { } website)
            return BadRequest(new { message = "Unknown or missing website." });

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var (status, client) = await _auth.ValidateCredentialsAsync(
            website.WebsiteID, request.Identifier, request.Password, ct);

        await _loginLog.RecordAsync(request.Identifier, website.WebsiteID, status == ClientLoginStatus.Success, ip, ct);

        return status switch
        {
            ClientLoginStatus.Success when client is not null => Ok(IssueToken(client)),
            ClientLoginStatus.NotActivated => StatusCode(StatusCodes.Status403Forbidden, new
            {
                status = "NotActivated",
                identifier = request.Identifier,
                message = "Your account is not activated yet. Please enter the code we sent you.",
            }),
            _ => Unauthorized(new { message = "Invalid credentials." }),
        };
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] IdentifierRequest request, CancellationToken ct = default)
    {
        if (await ResolveWebsiteAsync(ct) is not { } website)
            return BadRequest(new { message = "Unknown or missing website." });

        var result = await _auth.RequestPasswordResetAsync(website.WebsiteID, request.Identifier, ct);
        // Don't reveal whether the account exists; report a generic success unless delivery is impossible.
        return result switch
        {
            ClientResetRequestResult.DeliveryNotConfigured => StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Reset email cannot be sent — email is not configured for this website.",
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

    private object IssueToken(WebsiteClient client)
    {
        var token = _tokenService.CreateToken(client);
        return new { accessToken = token.AccessToken, expiresAtUtc = token.ExpiresAtUtc, tokenType = "Bearer" };
    }

    private async Task<Website?> ResolveWebsiteAsync(CancellationToken ct)
    {
        if (!Request.Headers.TryGetValue(WebsiteKeyHeader, out var keyValue) ||
            !Guid.TryParse(keyValue.ToString(), out var authCode))
            return null;

        var website = await _websiteService.GetByAuthCodeAsync(authCode, ct);
        return website is { Active: true } ? website : null;
    }
}
