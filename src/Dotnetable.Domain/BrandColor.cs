namespace Dotnetable.Domain;

/// <summary>
/// Public brand color stored on a website and returned by <c>api/siteinfo</c>.
/// Empty means the theme keeps its own default. Only <c>#rgb</c> and <c>#rrggbb</c> are accepted.
/// </summary>
public static class BrandColor
{
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var text = value.Trim();
        if (text.Length == 4 && text[0] == '#')
        {
            text = string.Create(7, text, static (span, source) =>
            {
                span[0] = '#';
                span[1] = source[1];
                span[2] = source[1];
                span[3] = source[2];
                span[4] = source[2];
                span[5] = source[3];
                span[6] = source[3];
            });
        }

        if (text.Length != 7 || text[0] != '#')
            return null;

        for (var i = 1; i < 7; i++)
        {
            var c = text[i];
            var hex = c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
            if (!hex)
                return null;
        }

        return text.ToLowerInvariant();
    }
}
