using System.Globalization;

namespace Dotnetable.Admin.Components.Shared.FormControls;

/// <summary>
/// Consistent money display and MudNumericField formatting (thousand separators).
/// Uses invariant digits with comma grouping so IRR-style amounts read as 1,250,000.
/// IRR (ریال) and IRT (تومان) always display with zero decimal places.
/// </summary>
public static class MoneyFormat
{
    public static CultureInfo DisplayCulture { get; } = CreateCulture();

    private static CultureInfo CreateCulture()
    {
        var c = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        c.NumberFormat.NumberGroupSeparator = ",";
        c.NumberFormat.NumberDecimalSeparator = ".";
        c.NumberFormat.NumberGroupSizes = [3];
        return c;
    }

    /// <summary>True for Iranian rial / toman codes that must never show fractional units.</summary>
    public static bool IsZeroDecimalCurrency(string? currencyCode) =>
        string.Equals(currencyCode, "IRR", StringComparison.OrdinalIgnoreCase)
        || string.Equals(currencyCode, "IRT", StringComparison.OrdinalIgnoreCase);

    /// <summary>Apply currency-specific rules (IRR/IRT → 0 digits) on top of catalog decimal digits.</summary>
    public static int EffectiveDigits(string? currencyCode, int decimalDigits = 0) =>
        IsZeroDecimalCurrency(currencyCode) ? 0 : Math.Clamp(decimalDigits, 0, 6);

    /// <summary>MudNumericField Format string (e.g. N0, N2).</summary>
    public static string FieldFormat(int decimalDigits = 0) =>
        decimalDigits <= 0 ? "N0" : $"N{Math.Clamp(decimalDigits, 0, 6)}";

    /// <summary>MudNumericField Format using currency rules (IRR/IRT force N0).</summary>
    public static string FieldFormat(string? currencyCode, int decimalDigits = 0) =>
        FieldFormat(EffectiveDigits(currencyCode, decimalDigits));

    /// <summary>Format a money amount with thousand separators.</summary>
    public static string Group(decimal value, int decimalDigits = 0) =>
        value.ToString(FieldFormat(decimalDigits), DisplayCulture);

    /// <summary>Format with currency rules (IRR/IRT without decimals).</summary>
    public static string Group(decimal value, string? currencyCode, int decimalDigits = 0) =>
        Group(value, EffectiveDigits(currencyCode, decimalDigits));

    /// <summary>Format with optional currency code (no $ prefix).</summary>
    public static string GroupWithCode(decimal value, string? currencyCode, int decimalDigits = 0)
    {
        var text = Group(value, currencyCode, decimalDigits);
        return string.IsNullOrWhiteSpace(currencyCode) ? text : $"{text} {currencyCode.Trim()}";
    }
}
