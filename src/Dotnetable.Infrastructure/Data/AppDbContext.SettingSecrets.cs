using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetable.Infrastructure.Data;

public partial class AppDbContext
{
    private readonly byte[]? _settingKey;

    /// <summary>Derived key for reversible setting encryption. Null when Internal has no real secret.</summary>
    public byte[]? SettingProtectionKey => _settingKey;

    private void ApplySettingSecrets(ModelBuilder modelBuilder)
    {
        // Ciphertext of a full previous value does not fit nvarchar(4000) (SQL Server's cap is 4000
        // characters). Drop the cap so each provider stores these as unbounded text.
        Widen(modelBuilder.Entity<WebsiteSmsSetting>().Property(e => e.SettingsJSON));
        Widen(modelBuilder.Entity<WebsiteWhatsAppSetting>().Property(e => e.SettingsJSON));
        Widen(modelBuilder.Entity<MarketplaceChannel>().Property(e => e.SettingsJSON));
        Widen(modelBuilder.Entity<WebsiteStorageSetting>().Property(e => e.StorageSettingsJSON));
        Widen(modelBuilder.Entity<EmailAccount>().Property(e => e.Password));
        Widen(modelBuilder.Entity<PaymentGateway>().Property(e => e.MerchantID));
        Widen(modelBuilder.Entity<PaymentGateway>().Property(e => e.ApiKey));
        Widen(modelBuilder.Entity<PaymentGateway>().Property(e => e.ApiSecret));
        Widen(modelBuilder.Entity<PaymentGateway>().Property(e => e.SettingsJSON));
        Widen(modelBuilder.Entity<WebsiteCaptchaSetting>().Property(e => e.TurnstileSecretKey));

        if (_settingKey is null) return;

        var key = _settingKey;
        Protect(modelBuilder.Entity<WebsiteSmsSetting>().Property(e => e.SettingsJSON), key);
        Protect(modelBuilder.Entity<WebsiteWhatsAppSetting>().Property(e => e.SettingsJSON), key);
        Protect(modelBuilder.Entity<MarketplaceChannel>().Property(e => e.SettingsJSON), key);
        Protect(modelBuilder.Entity<WebsiteStorageSetting>().Property(e => e.StorageSettingsJSON), key);
        Protect(modelBuilder.Entity<EmailAccount>().Property(e => e.Password), key);
        Protect(modelBuilder.Entity<PaymentGateway>().Property(e => e.MerchantID)!, key);
        Protect(modelBuilder.Entity<PaymentGateway>().Property(e => e.ApiKey)!, key);
        Protect(modelBuilder.Entity<PaymentGateway>().Property(e => e.ApiSecret)!, key);
        Protect(modelBuilder.Entity<PaymentGateway>().Property(e => e.SettingsJSON)!, key);
        Protect(modelBuilder.Entity<WebsiteCaptchaSetting>().Property(e => e.TurnstileSecretKey)!, key);
    }

    private static void Widen<T>(PropertyBuilder<T> property) => property.Metadata.SetMaxLength(null);

    private static void Protect(PropertyBuilder<string> property, byte[] key) =>
        property.HasConversion(
            v => SettingProtector.Protect(v, key)!,
            v => SettingProtector.Unprotect(v, key)!);
}
