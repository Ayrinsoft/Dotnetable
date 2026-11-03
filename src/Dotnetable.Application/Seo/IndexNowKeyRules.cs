namespace Dotnetable.Application.Seo;

/// <summary>
/// IndexNow key rules from https://www.indexnow.org/documentation :
/// 8–128 characters, only ASCII letters, digits, and dashes.
/// </summary>
public static class IndexNowKeyRules
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    public static bool IsValid(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        var value = key.Trim();
        if (value.Length is < MinLength or > MaxLength) return false;
        foreach (var c in value)
        {
            if (c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '-')
                continue;
            return false;
        }

        return true;
    }

    /// <summary>32 hexadecimal characters, which satisfies the IndexNow key grammar.</summary>
    public static string Generate() => Guid.NewGuid().ToString("N");
}
