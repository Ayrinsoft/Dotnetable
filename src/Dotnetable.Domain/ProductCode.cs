using System.Text.RegularExpressions;

namespace Dotnetable.Domain;

/// <summary>
/// Site product codes: <c>{PREFIX}-{ProductID}</c> (product, not variant).
/// Default prefix is <see cref="DefaultPrefix"/> (<c>DN</c>). Each website may set 1–3 Latin letters.
/// </summary>
public static partial class ProductCode
{
    public const string DefaultPrefix = "DN";
    public const int MinPrefixLength = 1;
    public const int MaxPrefixLength = 3;

    /// <summary>Normalize admin input to 1–3 uppercase A–Z letters; empty/invalid → <see cref="DefaultPrefix"/>.</summary>
    public static string NormalizePrefix(string? prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return DefaultPrefix;

        var letters = new string(prefix.Trim().Where(char.IsLetter).Take(MaxPrefixLength).ToArray());
        if (letters.Length < MinPrefixLength)
            return DefaultPrefix;

        return letters.ToUpperInvariant();
    }

    /// <summary>Build <c>DN-42</c> style code for a product id.</summary>
    public static string Format(string? prefix, int productId)
    {
        if (productId <= 0) return "";
        return $"{NormalizePrefix(prefix)}-{productId}";
    }

    /// <summary>
    /// Parse <c>DN-42</c> or <c>dn-42</c>. Returns false for slugs without the numeric product id tail.
    /// </summary>
    public static bool TryParse(string? input, out string prefix, out int productId)
    {
        prefix = "";
        productId = 0;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var m = CodeRegex().Match(input.Trim());
        if (!m.Success) return false;

        prefix = m.Groups[1].Value.ToUpperInvariant();
        return int.TryParse(m.Groups[2].Value, out productId) && productId > 0;
    }

    /// <summary>True when the string looks like a product code (not a normal SEO slug).</summary>
    public static bool LooksLikeCode(string? input) => TryParse(input, out _, out _);

    [GeneratedRegex(@"^([A-Za-z]{1,3})-(\d+)$", RegexOptions.CultureInvariant)]
    private static partial Regex CodeRegex();
}
