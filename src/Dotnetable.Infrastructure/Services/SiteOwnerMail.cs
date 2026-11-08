using Dotnetable.Application;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// The address an admin alert is emailed to: the website's own <c>Website.Email</c>, or website 1's
/// when this site has none. No address on either site means the email is skipped.
/// </summary>
internal static class SiteOwnerMail
{
    internal sealed record Target(string Email, string Name, string? LanguageCode);

    internal static async Task<Target?> ResolveAsync(AppDbContext context, int websiteId, CancellationToken ct)
    {
        var rows = await context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId || w.WebsiteID == AppConstants.MasterWebsiteId)
            .Select(w => new Row(w.WebsiteID, w.Email, w.Manager, w.DefaultLanguageCode))
            .ToListAsync(ct);

        var own = rows.FirstOrDefault(w => w.WebsiteID == websiteId);
        if (AddressOf(own) is string ownEmail)
            return new Target(ownEmail, NameOf(own!, ownEmail), LanguageOf(own!));

        if (websiteId == AppConstants.MasterWebsiteId)
            return null;

        var master = rows.FirstOrDefault(w => w.WebsiteID == AppConstants.MasterWebsiteId);
        if (AddressOf(master) is string masterEmail)
            return new Target(masterEmail, NameOf(master!, masterEmail), LanguageOf(master!));

        return null;
    }

    private static string? AddressOf(Row? row) =>
        row is null || string.IsNullOrWhiteSpace(row.Email) ? null : row.Email.Trim();

    private static string NameOf(Row row, string email) =>
        string.IsNullOrWhiteSpace(row.Manager) ? email : row.Manager.Trim();

    private static string? LanguageOf(Row row) =>
        string.IsNullOrWhiteSpace(row.LanguageCode) ? null : row.LanguageCode.Trim();

    private sealed record Row(int WebsiteID, string Email, string Manager, string LanguageCode);
}
