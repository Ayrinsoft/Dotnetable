using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dotnetable.Application.Authorization;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Dotnetable.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(JwtSettings settings) => _settings = settings;

    public JwtTokenResult CreateToken(Member member)
    {
        if (string.IsNullOrWhiteSpace(_settings.SigningKey))
            throw new InvalidOperationException("Jwt:SigningKey is not configured.");

        var expires = DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes);

        // Identity + role claims are shared with the admin cookie sign-in so a token means the same thing.
        var claims = new List<Claim>(MemberClaims.Build(member))
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new JwtTokenResult(accessToken, expires);
    }
}
