using System.Net;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Admin.Components.Pages.Media;

/// <summary>
/// Builds HTML snippets for embedding a media library file into CKEditor content.
/// </summary>
public static class MediaEmbedHtml
{
    /// <summary>
    /// Returns an HTML fragment for the given file, or null when no public URL is available.
    /// </summary>
    public static string? ToHtml(FileRecord file)
    {
        var url = FirstNonEmpty(file.CNDUrl, file.ThumbnailCDN);
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var safeUrl = WebUtility.HtmlEncode(url);
        var alt = WebUtility.HtmlEncode(FirstNonEmpty(file.AltText, file.Title, file.OriginalFileName) ?? "");
        var title = WebUtility.HtmlEncode(FirstNonEmpty(file.Title, file.OriginalFileName) ?? "");
        var category = (FileCategory)file.FileCategory;

        return category switch
        {
            FileCategory.Image =>
                $"<p><img src=\"{safeUrl}\" alt=\"{alt}\" title=\"{title}\" /></p>",

            FileCategory.Video =>
                $"<p><video controls src=\"{safeUrl}\" style=\"max-width:100%;\">" +
                $"<a href=\"{safeUrl}\">{title}</a></video></p>",

            FileCategory.Audio =>
                $"<p><audio controls src=\"{safeUrl}\">" +
                $"<a href=\"{safeUrl}\">{title}</a></audio></p>",

            _ =>
                $"<p><a href=\"{safeUrl}\" target=\"_blank\" rel=\"noopener noreferrer\">{title}</a></p>",
        };
    }

    /// <summary>Appends the embed HTML after the current editor content.</summary>
    public static string Append(string? currentHtml, string embedHtml)
    {
        if (string.IsNullOrWhiteSpace(currentHtml))
            return embedHtml;
        return currentHtml.TrimEnd() + "\n" + embedHtml;
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
                return v;
        }
        return null;
    }
}
