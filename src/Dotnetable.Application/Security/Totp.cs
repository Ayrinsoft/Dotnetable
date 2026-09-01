using System.Security.Cryptography;
using System.Text;

namespace Dotnetable.Application.Security;

/// <summary>
/// RFC 6238 time-based one-time passwords — the six-digit code from Google Authenticator, Authy,
/// 1Password and every other authenticator app.
///
/// <para>Implemented here rather than pulled from a package because the algorithm is small, stable
/// and fully specified, and because ASP.NET Core's own implementation is internal to Identity, which
/// this product does not use for admin members.</para>
/// </summary>
public static class Totp
{
    /// <summary>Standard step. Every authenticator app assumes 30 seconds.</summary>
    private const int PeriodSeconds = 30;

    private const int Digits = 6;

    /// <summary>
    /// How many steps either side of "now" are accepted. One step covers ordinary clock drift between
    /// the phone and the server without meaningfully widening the guessing window.
    /// </summary>
    private const int AllowedDrift = 1;

    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    /// <summary>Generates a fresh Base32 shared secret (160 bits, the RFC 4226 recommendation).</summary>
    public static string GenerateSecret() => ToBase32(RandomNumberGenerator.GetBytes(20));

    /// <summary>
    /// Verifies a submitted code against the secret, accepting <see cref="AllowedDrift"/> steps of
    /// clock skew. Comparison is length-constant so a near-miss leaks no timing signal.
    /// </summary>
    public static bool Verify(string? base32Secret, string? code, DateTime? utcNow = null)
    {
        if (string.IsNullOrWhiteSpace(base32Secret) || string.IsNullOrWhiteSpace(code))
            return false;

        var digits = new string(code.Where(char.IsDigit).ToArray());
        if (digits.Length != Digits) return false;

        byte[] key;
        try
        {
            key = FromBase32(base32Secret);
        }
        catch (FormatException)
        {
            return false;
        }

        var step = (long)((utcNow ?? DateTime.UtcNow) - UnixEpoch).TotalSeconds / PeriodSeconds;

        for (var offset = -AllowedDrift; offset <= AllowedDrift; offset++)
        {
            var candidate = Compute(key, step + offset);
            if (CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(candidate), Encoding.ASCII.GetBytes(digits)))
                return true;
        }

        return false;
    }

    /// <summary>
    /// The <c>otpauth://</c> URI an authenticator app scans. <paramref name="issuer"/> and
    /// <paramref name="account"/> are what the user sees in their app's list, so they need to name
    /// the shop and the login, not an internal id.
    /// </summary>
    public static string BuildProvisioningUri(string base32Secret, string issuer, string account)
    {
        var label = $"{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(account)}";
        return $"otpauth://totp/{label}" +
               $"?secret={base32Secret}" +
               $"&issuer={Uri.EscapeDataString(issuer)}" +
               $"&algorithm=SHA1&digits={Digits}&period={PeriodSeconds}";
    }

    private static string Compute(byte[] key, long step)
    {
        var counter = BitConverter.GetBytes(step);
        if (BitConverter.IsLittleEndian) Array.Reverse(counter);

        var hash = HMACSHA1.HashData(key, counter);

        // Dynamic truncation (RFC 4226 §5.3): the low nibble of the last byte picks the 4-byte window.
        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24)
                     | ((hash[offset + 1] & 0xFF) << 16)
                     | ((hash[offset + 2] & 0xFF) << 8)
                     | (hash[offset + 3] & 0xFF);

        return (binary % 1_000_000).ToString("D6");
    }

    private static string ToBase32(byte[] data)
    {
        var builder = new StringBuilder();
        int buffer = 0, bitsLeft = 0;

        foreach (var b in data)
        {
            buffer = (buffer << 8) | b;
            bitsLeft += 8;
            while (bitsLeft >= 5)
            {
                builder.Append(Base32Alphabet[(buffer >> (bitsLeft - 5)) & 31]);
                bitsLeft -= 5;
            }
        }

        if (bitsLeft > 0)
            builder.Append(Base32Alphabet[(buffer << (5 - bitsLeft)) & 31]);

        return builder.ToString();
    }

    private static byte[] FromBase32(string value)
    {
        // Authenticator apps display secrets in spaced, lower-cased groups; accept them back as typed.
        var normalized = value.Replace(" ", "").Replace("-", "").TrimEnd('=').ToUpperInvariant();

        var bytes = new List<byte>(normalized.Length * 5 / 8);
        int buffer = 0, bitsLeft = 0;

        foreach (var c in normalized)
        {
            var index = Base32Alphabet.IndexOf(c);
            if (index < 0) throw new FormatException($"'{c}' is not a Base32 character.");

            buffer = (buffer << 5) | index;
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                bytes.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
                bitsLeft -= 8;
            }
        }

        return bytes.ToArray();
    }
}
