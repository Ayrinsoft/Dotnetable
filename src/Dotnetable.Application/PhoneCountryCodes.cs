namespace Dotnetable.Application;

/// <summary>
/// Dialing codes for customer/member phones. Defaults come from each website's main mobile
/// (and optionally tax-country prefix) — never hard-code a single country.
/// </summary>
public static class PhoneCountryCodes
{
    /// <summary>Digits only; strips leading + / 00.</summary>
    public static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("00", StringComparison.Ordinal))
            digits = digits[2..];
        return string.IsNullOrEmpty(digits) ? null : digits;
    }

    /// <summary>
    /// Resolve the default phone country code for a website from its main mobile number,
    /// matching against known country prefixes (longest first). Local numbers (leading 0)
    /// fall back to the tax-jurisdiction phone prefix.
    /// </summary>
    public static string? FromWebsiteMobile(
        string? websiteMobile,
        IEnumerable<string?> knownPrefixes,
        string? taxCountryPhonePrefix = null)
    {
        var prefixes = knownPrefixes
            .Select(Normalize)
            .Where(p => !string.IsNullOrEmpty(p))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .OrderByDescending(p => p.Length)
            .ToList();

        var tax = Normalize(taxCountryPhonePrefix);
        var digits = Normalize(websiteMobile);
        if (string.IsNullOrEmpty(digits))
            return tax;

        // Local national format (e.g. 0912…) — use tax-country dialing code when available.
        if (digits.StartsWith('0'))
            return tax;

        foreach (var prefix in prefixes)
        {
            if (digits.StartsWith(prefix, StringComparison.Ordinal))
                return prefix;
        }

        return tax;
    }
}
