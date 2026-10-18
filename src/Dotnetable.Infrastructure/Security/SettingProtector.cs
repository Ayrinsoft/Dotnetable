using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnetable.Infrastructure.Security;

/// <summary>
/// Reversible encryption for credentials stored in the database (SMS, WhatsApp, payment, mail,
/// storage, marketplace, captcha secret). Keyed from <c>Internal:ProtectionKey</c> when that is set,
/// otherwise from the shared <c>Internal:SyncSecret</c> Admin and the API already agree on.
/// The raw secret is never the AES key: a 32-byte key is derived with HKDF so the cache-sync header
/// and the ciphertext key are not the same bytes.
/// </summary>
public static class SettingProtector
{
    public const string Prefix = "enc1.";

    private const int NonceSize = 12;
    private const int TagSize = 16;

    /// <summary>
    /// Placeholder values shipped in source are not keys. A missing or placeholder secret leaves
    /// values in plaintext so a fresh checkout and the test suite keep working.
    /// </summary>
    public static byte[]? DeriveKey(string? protectionKey, string? syncSecret)
    {
        var secret = RealSecret(protectionKey) ?? RealSecret(syncSecret);
        if (secret is null) return null;

        return HKDF.DeriveKey(
            HashAlgorithmName.SHA256,
            Encoding.UTF8.GetBytes(secret),
            32,
            info: "Dotnetable.Settings.v1"u8.ToArray());
    }

    public static string? Protect(string? value, byte[] key)
    {
        if (string.IsNullOrEmpty(value) || value.StartsWith(Prefix, StringComparison.Ordinal))
            return value;

        var plain = Encoding.UTF8.GetBytes(value);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[plain.Length];
        var tag = new byte[TagSize];
        using (var aes = new AesGcm(key, TagSize))
            aes.Encrypt(nonce, plain, cipher, tag);

        var payload = new byte[NonceSize + TagSize + cipher.Length];
        nonce.CopyTo(payload, 0);
        tag.CopyTo(payload, NonceSize);
        cipher.CopyTo(payload, NonceSize + TagSize);
        return Prefix + Convert.ToBase64String(payload);
    }

    public static string? Unprotect(string? value, byte[] key)
    {
        if (string.IsNullOrEmpty(value) || !value.StartsWith(Prefix, StringComparison.Ordinal))
            return value;

        byte[] payload;
        try
        {
            payload = Convert.FromBase64String(value[Prefix.Length..]);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("Stored setting secret is not valid ciphertext.", ex);
        }

        if (payload.Length < NonceSize + TagSize)
            throw new InvalidOperationException("Stored setting secret is not valid ciphertext.");

        var nonce = payload.AsSpan(0, NonceSize);
        var tag = payload.AsSpan(NonceSize, TagSize);
        var cipher = payload.AsSpan(NonceSize + TagSize);
        var plain = new byte[cipher.Length];
        try
        {
            using var aes = new AesGcm(key, TagSize);
            aes.Decrypt(nonce, cipher, tag, plain);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException(
                "Could not decrypt a stored setting. Internal:ProtectionKey (or Internal:SyncSecret, when no protection key is set) does not match the key that encrypted it.",
                ex);
        }

        return Encoding.UTF8.GetString(plain);
    }

    private static string? RealSecret(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (value.StartsWith("dev-only-change-me", StringComparison.Ordinal)) return null;
        if (value.Length < 16) return null;
        return value;
    }
}

/// <summary>Carries the derived settings key on a context so encryption stays per-process and out of the test suite.</summary>
public sealed class SettingProtectionOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;

    public SettingProtectionOptionsExtension(byte[] key) => Key = key;

    public byte[] Key { get; }

    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services)
    {
    }

    public void Validate(IDbContextOptions options)
    {
    }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        private readonly SettingProtectionOptionsExtension _extension;

        public ExtensionInfo(SettingProtectionOptionsExtension extension)
            : base(extension) => _extension = extension;

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "SettingProtection ";

        public override int GetServiceProviderHashCode()
        {
            var hash = new HashCode();
            hash.AddBytes(_extension.Key);
            return hash.ToHashCode();
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) =>
            other is ExtensionInfo info && info._extension.Key.AsSpan().SequenceEqual(_extension.Key);

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) =>
            debugInfo["SettingProtection"] = "1";
    }
}

public static class SettingProtectionOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseSettingProtection(this DbContextOptionsBuilder options, byte[] key)
    {
        ((IDbContextOptionsBuilderInfrastructure)options).AddOrUpdateExtension(new SettingProtectionOptionsExtension(key));
        return options;
    }

    public static DbContextOptionsBuilder<TContext> UseSettingProtection<TContext>(
        this DbContextOptionsBuilder<TContext> options,
        byte[] key)
        where TContext : DbContext
    {
        ((IDbContextOptionsBuilderInfrastructure)options).AddOrUpdateExtension(new SettingProtectionOptionsExtension(key));
        return options;
    }
}
