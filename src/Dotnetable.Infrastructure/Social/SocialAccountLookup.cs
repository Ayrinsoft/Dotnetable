using Dotnetable.Application.Instagram;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Social;

internal static class SocialAccountLookup
{
    public static Task<WebsiteInstagramAccount> InstagramAsync(
        AppDbContext context, int websiteId, int? accountId, CancellationToken ct) =>
        OneAsync(
            context.WebsiteInstagramAccounts.AsNoTracking().Where(a => a.WebsiteID == websiteId && a.IsActive),
            accountId,
            a => a.WebsiteInstagramAccountID,
            "Connect an Instagram account on this page first.",
            "Choose which Instagram channel to send to.",
            "That Instagram channel was not found.",
            ct);

    public static Task<WebsiteTelegramAccount> TelegramAsync(
        AppDbContext context, int websiteId, int? accountId, CancellationToken ct) =>
        OneAsync(
            context.WebsiteTelegramAccounts.AsNoTracking().Where(a => a.WebsiteID == websiteId && a.IsActive),
            accountId,
            a => a.WebsiteTelegramAccountID,
            "Connect a Telegram bot on this page first.",
            "Choose which Telegram channel to send to.",
            "That Telegram channel was not found.",
            ct);

    public static Task<WebsiteXAccount> XAsync(
        AppDbContext context, int websiteId, int? accountId, CancellationToken ct) =>
        OneAsync(
            context.WebsiteXAccounts.AsNoTracking().Where(a => a.WebsiteID == websiteId && a.IsActive),
            accountId,
            a => a.WebsiteXAccountID,
            "Connect an X account on this page first.",
            "Choose which X channel to send to.",
            "That X channel was not found.",
            ct);

    private static async Task<T> OneAsync<T>(
        IQueryable<T> active,
        int? accountId,
        System.Linq.Expressions.Expression<Func<T, int>> id,
        string missing,
        string choose,
        string notFound,
        CancellationToken ct)
        where T : class
    {
        if (accountId is int wanted)
        {
            var match = await active.FirstOrDefaultAsync(RowEquals(id, wanted), ct);
            return match ?? throw new SocialPublishException(notFound);
        }

        var rows = await active.OrderBy(id).Take(2).ToListAsync(ct);
        if (rows.Count == 0) throw new SocialPublishException(missing);
        if (rows.Count > 1) throw new SocialPublishException(choose);
        return rows[0];
    }

    private static System.Linq.Expressions.Expression<Func<T, bool>> RowEquals<T>(
        System.Linq.Expressions.Expression<Func<T, int>> id, int wanted)
    {
        var parameter = id.Parameters[0];
        var body = System.Linq.Expressions.Expression.Equal(id.Body, System.Linq.Expressions.Expression.Constant(wanted));
        return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
