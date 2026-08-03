using System.Globalization;

namespace Dotnetable.Admin.Components.Shared.FormControls;

/// <summary>
/// Consistent money display and MudNumericField formatting (thousand separators).
/// Uses invariant digits with comma grouping so IRR-style amounts read as 1,250,000.
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

    /// <summary>MudNumericField Format string (e.g. N0, N2).</summary>
    public static string FieldFormat(int decimalDigits = 0) =>
        decimalDigits <= 0 ? "N0" : $"N{Math.Clamp(decimalDigits, 0, 6)}";

    /// <summary>Format a money amount with thousand separators.</summary>
    public static string Group(decimal value, int decimalDigits = 0) =>
        value.ToString(FieldFormat(decimalDigits), DisplayCulture);

    /// <summary>Format with optional currency code (no $ prefix).</summary>
    public static string GroupWithCode(decimal value, string? currencyCode, int decimalDigits = 0)
    {
        var text = Group(value, decimalDigits);
        return string.IsNullOrWhiteSpace(currencyCode) ? text : $"{text} {currencyCode.Trim()}";
    }
}
