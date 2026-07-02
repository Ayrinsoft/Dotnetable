using System.Security.Claims;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Authorization;

/// <summary>
/// Claims that identify a signed-in website customer (<see cref="WebsiteClient"/>) in an API JWT.
/// Customers have no policy/roles — only a loyalty level — so the token carries the fixed set of
/// client permission keys plus the customer's <see cref="WebsiteClient.ClientLevel"/>.
/// </summary>
public static class ClientClaims
{
    /// <summary>WebsiteClientID of the customer.</summary>
    public const string ClientId = "cid";

    /// <summary>The customer's loyalty tier (<see cref="Domain.Enums.ClientLevel"/> name).</summary>
    public const string ClientLevel = "clevel";

    /// <summary>Marks the token as belonging to a customer rather than an admin member.</summary>
    public const string IsClient = "client";

    public static IReadOnlyList<Claim> Build(WebsiteClient client)
    {
        var name = client.Email ?? client.Cellphone ?? client.WebsiteClientID.ToString();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, client.WebsiteClientID.ToString()),
            new(ClaimTypes.Name, name),
            new(ClientId, client.WebsiteClientID.ToString()),
            new(MemberClaims.WebsiteId, client.WebsiteID.ToString()),
            new(ClientLevel, client.ClientLevel.ToString()),
            new(IsClient, "true"),
        };

        if (!string.IsNullOrWhiteSpace(client.Email))
            claims.Add(new Claim(ClaimTypes.Email, client.Email));

        // Customers get every client-category permission key (there is no per-customer policy).
        claims.AddRange(RoleCatalog.ClientKeys.Select(key => new Claim(ClaimTypes.Role, key)));

        return claims;
    }
}
