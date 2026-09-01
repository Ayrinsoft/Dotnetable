using System.Text.RegularExpressions;

namespace Dotnetable.Application.Security;

/// <summary>
/// The one place that decides whether a password is acceptable, shared by customer registration and
/// reset (API) and by admin member create/change (panel), so the storefront and the panel can never
/// drift apart on what "strong enough" means.
/// </summary>
public static partial class PasswordPolicy
{
    /// <summary>Minimum length for a customer account.</summary>
    public const int MinimumLength = 10;

    /// <summary>Minimum length for an admin member, who can read every order in the shop.</summary>
    public const int AdminMinimumLength = 12;

    /// <summary>Longest accepted password. Bounded so a megabyte of text can't be fed to the hasher.</summary>
    public const int MaximumLength = 256;

    /// <summary>
    /// Passwords rejected outright regardless of length/character mix. Kept deliberately short —
    /// this is a speed bump for the obvious cases, not a substitute for a breach-corpus check.
    /// </summary>
    private static readonly HashSet<string> Forbidden = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "password1", "password123", "passw0rd", "p@ssw0rd", "qwerty123", "123456789",
        "1234567890", "iloveyou", "adminadmin", "administrator", "letmein123", "welcome123",
        "changeme123", "dotnetable", "dotnetable1", "test1234", "abcd1234", "qwertyuiop",
    };

    [GeneratedRegex(@"\p{Lu}")] private static partial Regex Upper();
    [GeneratedRegex(@"\p{Ll}")] private static partial Regex Lower();
    [GeneratedRegex(@"\d")] private static partial Regex Digit();
    [GeneratedRegex(@"[^\p{L}\d]")] private static partial Regex Symbol();

    /// <summary>The outcome of a policy check. <see cref="Error"/> is null when the password passes.</summary>
    public readonly record struct Result(bool Ok, string? Error)
    {
        public static readonly Result Success = new(true, null);
        public static Result Fail(string message) => new(false, message);
    }

    /// <summary>
    /// Validates a customer password. <paramref name="identifiers"/> are the values the password must
    /// not simply repeat (email, mobile, username) — reusing your own address as a password is one of
    /// the most common credential-stuffing wins.
    /// </summary>
    public static Result Validate(string? password, params string?[] identifiers) =>
        ValidateCore(password, MinimumLength, identifiers);

    /// <summary>Validates an admin member password against the stricter admin minimum.</summary>
    public static Result ValidateAdmin(string? password, params string?[] identifiers) =>
        ValidateCore(password, AdminMinimumLength, identifiers);

    private static Result ValidateCore(string? password, int minLength, string?[] identifiers)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result.Fail("Password is required.");

        if (password.Length < minLength)
            return Result.Fail($"Password must be at least {minLength} characters.");

        if (password.Length > MaximumLength)
            return Result.Fail($"Password must be at most {MaximumLength} characters.");

        var classes = 0;
        if (Upper().IsMatch(password)) classes++;
        if (Lower().IsMatch(password)) classes++;
        if (Digit().IsMatch(password)) classes++;
        if (Symbol().IsMatch(password)) classes++;

        if (classes < 3)
            return Result.Fail("Password must combine at least three of: uppercase, lowercase, digits, symbols.");

        if (Forbidden.Contains(password.Trim()))
            return Result.Fail("This password is too common. Please choose another one.");

        // A password that only repeats one character ("aaaaaaaaaaaa") passes the class check only by
        // accident; reject anything with fewer than five distinct characters.
        if (password.Distinct().Count() < 5)
            return Result.Fail("Password is too repetitive. Please choose another one.");

        foreach (var identifier in identifiers)
        {
            if (string.IsNullOrWhiteSpace(identifier) || identifier.Trim().Length < 4) continue;
            var needle = identifier.Trim();
            if (password.Contains(needle, StringComparison.OrdinalIgnoreCase))
                return Result.Fail("Password must not contain your email, mobile number or username.");
        }

        return Result.Success;
    }
}
