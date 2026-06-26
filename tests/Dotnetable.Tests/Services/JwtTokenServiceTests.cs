using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dotnetable.Application;
using Dotnetable.Application.Authorization;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace Dotnetable.Tests.Services;

public class JwtTokenServiceTests
{
    private static readonly JwtSettings Settings = new()
    {
        Issuer = "Dotnetable",
        Audience = "Dotnetable.Clients",
        SigningKey = "test-signing-key-0123456789-abcdefghijklmno",
        AccessTokenMinutes = 60,
    };

    private static Member ClientMember() => new()
    {
        MemberID = 42,
        Username = "buyer",
        Email = "buyer@example.com",
        WebsiteID = 7, // non-master
        PolicyID = 3,
        Policy = new Policy
        {
            PolicyID = 3,
            PolicyRoles = new List<PolicyRole>
            {
                new() { Active = true, Role = new Role { RoleKey = RoleKeys.ClientPurchase, Active = true } },
                new() { Active = true, Role = new Role { RoleKey = RoleKeys.ClientReview, Active = true } },
                new() { Active = false, Role = new Role { RoleKey = "client.disabled", Active = true } },
            },
        },
    };

    [Fact]
    public void CreateToken_EmbedsIdentityAndActiveRoleClaims()
    {
        var token = new JwtTokenService(Settings).CreateToken(ClientMember());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);

        jwt.Issuer.Should().Be(Settings.Issuer);
        jwt.Claims.Should().Contain(c => c.Type == MemberClaims.WebsiteId && c.Value == "7");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == RoleKeys.ClientPurchase);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == RoleKeys.ClientReview);
        jwt.Claims.Should().NotContain(c => c.Type == ClaimTypes.Role && c.Value == "client.disabled");
        jwt.Claims.Should().NotContain(c => c.Type == MemberClaims.Master);
        token.ExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void CreateToken_MasterWebsiteMember_GetsMasterClaim()
    {
        var member = ClientMember();
        member.WebsiteID = AppConstants.MasterWebsiteId;

        var token = new JwtTokenService(Settings).CreateToken(member);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);

        jwt.Claims.Should().Contain(c => c.Type == MemberClaims.Master && c.Value == "true");
    }

    [Fact]
    public void CreateToken_NoSigningKey_Throws()
    {
        var service = new JwtTokenService(new JwtSettings { SigningKey = "" });
        service.Invoking(s => s.CreateToken(ClientMember()))
            .Should().Throw<InvalidOperationException>();
    }
}
