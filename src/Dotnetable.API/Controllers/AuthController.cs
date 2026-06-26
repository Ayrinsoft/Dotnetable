using System.ComponentModel.DataAnnotations;
using Dotnetable.Application.Authorization;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Token endpoint for website clients (customers). They authenticate with their member credentials
/// and receive a JWT carrying their role claims; subsequent API calls send it as a bearer token.
/// </summary>
public class AuthController : BaseController
{
    private readonly IMemberService _memberService;
    private readonly IJwtTokenService _tokenService;
    private readonly ILoginLogService _loginLog;

    public AuthController(IMemberService memberService, IJwtTokenService tokenService, ILoginLogService loginLog)
    {
        _memberService = memberService;
        _tokenService = tokenService;
        _loginLog = loginLog;
    }

    public sealed class LoginRequest
    {
        [Required] public string Username { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
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
