using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Social;

internal static class SocialContentText
{
    /// <summary>
    /// Summary for the chosen language. A translation row uses its own text, including a blank one.
    /// No row keeps the primary summary.
    /// </summary>
    public static async Task<string> ExcerptAsync(
        AppDbContext context, bool product, int ownerId, string primary, string? languageCode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(languageCode)) return primary;
        if (product)
        {
            var hit = await context.ProductTranslations.AsNoTracking()
                .Where(t => t.ProductID == ownerId && t.LanguageCode == languageCode)
                .Select(t => new { t.ShortDescription })
                .FirstOrDefaultAsync(ct);
            return hit is null ? primary : hit.ShortDescription ?? "";
        }

        var post = await context.PostTranslations.AsNoTracking()
            .Where(t => t.PostID == ownerId && t.LanguageCode == languageCode)
            .Select(t => new { t.Excerpt })
            .FirstOrDefaultAsync(ct);
        return post is null ? primary : post.Excerpt ?? "";
    }
}
