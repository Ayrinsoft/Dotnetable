using System.ComponentModel.DataAnnotations;
using Dotnetable.Application.Authorization;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Token endpoint for website clients (customers). They authenticate with their member credentials
/// and receive a JWT carrying their role claims; subsequent API calls send it as a bearer token.
/// </summary>
public class AuthController : BaseController
{
    /// <summary>Header carrying the caller website's per-site key (<see cref="Website.AuthCode"/>).</summary>
    public const string WebsiteKeyHeader = "X-Website-Key";

    private readonly IMemberService _memberService;
    private readonly IJwtTokenService _tokenService;
    private readonly ILoginLogService _loginLog;
    private readonly IWebsiteService _websiteService;
    private readonly IPolicyService _policyService;

    public AuthController(
        IMemberService memberService,
        IJwtTokenService tokenService,
        ILoginLogService loginLog,
        IWebsiteService websiteService,
        IPolicyService policyService)
    {
        _memberService = memberService;
        _tokenService = tokenService;
        _loginLog = loginLog;
        _websiteService = websiteService;
        _policyService = policyService;
    }

    public sealed class LoginRequest
    {
        [Required] public string Username { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }

    public sealed class RegisterRequest
    {
        [Required] public string GivenName { get; set; } = string.Empty;
        [Required] public string Surname { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Username { get; set; } = string.Empty;
        [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct = default)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

        var member = await _memberService.ValidateCredentialsAsync(request.Username, request.Password, ct);
        if (member is null)
        {
            var websiteId = await _memberService.GetWebsiteIdByUsernameAsync(request.Username, ct) ?? 0;
            await _loginLog.RecordAsync(request.Username, websiteId, false, ip, ct);
            return Unauthorized(new { message = "Invalid username or password." });
        }

        await _loginLog.RecordAsync(member.Username, member.WebsiteID, true, ip, ct);

        var token = _tokenService.CreateToken(member);
        return Ok(new
        {
            accessToken = token.AccessToken,
            expiresAtUtc = token.ExpiresAtUtc,
            tokenType = "Bearer",
        });
    }

    /// <summary>
    /// Public self-registration for website customers. The target website is resolved from the
    /// <c>X-Website-Key</c> header (the site's <see cref="Website.AuthCode"/>); the new member is
    /// assigned that website's default "Users" access level.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct = default)
    {
        if (!Request.Headers.TryGetValue(WebsiteKeyHeader, out var keyValue) ||
            !Guid.TryParse(keyValue.ToString(), out var authCode))
        {
            return BadRequest(new { message = "Missing or invalid website key." });
        }

        var website = await _websiteService.GetByAuthCodeAsync(authCode, ct);
        if (website is null || !website.Active)
            return BadRequest(new { message = "Unknown website." });

        var username = request.Username.Trim();
        var email = request.Email.Trim();

        if (await _memberService.ExistsAsync(username, email, ct))
            return Conflict(new { message = "An account with this username or email already exists." });

        var policy = await _policyService.GetDefaultMemberPolicyAsync(website.WebsiteID, ct);
        if (policy is null)
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Registration is not configured for this website." });

        var member = new Member
        {
            WebsiteID = website.WebsiteID,
            PolicyID = policy.PolicyID,
            Username = username,
            Email = email,
            Givenname = request.GivenName.Trim(),
            Surname = request.Surname.Trim(),
            CellphoneNumber = string.Empty,
            CountryCode = string.Empty,
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            HashKey = Guid.NewGuid(),
            Active = true,
        };
        await _memberService.CreateAsync(member, request.Password, ct);

        return Ok(new { success = true, message = "Account created. You can sign in now." });
    }

    /// <summary>Returns the identity and granted role keys of the bearer-token caller.</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            username = User.Identity?.Name,
            websiteId = User.FindFirst(MemberClaims.WebsiteId)?.Value,
            isMaster = User.HasClaim(MemberClaims.Master, "true"),
            roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToArray(),
        });
    }
}
