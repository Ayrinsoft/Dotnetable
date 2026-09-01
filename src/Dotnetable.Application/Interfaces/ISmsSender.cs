using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Transactional SMS (OTP, shipment tracking, etc.), scoped to a website exactly like storage
/// backends: <c>WebsiteSmsSettings</c> holds one row per registered gateway and the active row with
/// the lowest sort order sends. This is the app-facing service — callers never pick a gateway.
/// </summary>
public interface ISmsSender
{
    /// <summary>
    /// True when <paramref name="websiteId"/> has an active gateway whose credentials are filled in.
    /// Registration and password reset both refuse to start an SMS flow when this is false, rather
    /// than reporting success for a message that will never arrive.
    /// </summary>
    Task<bool> IsConfiguredAsync(int websiteId, CancellationToken ct = default);

    /// <summary>
    /// Sends <paramref name="message"/> to a phone number (country code + national number) through
    /// the website's active gateway. Returns false when nothing was sent.
    /// </summary>
    Task<bool> SendAsync(int websiteId, string countryCode, string cellphone, string message,
        CancellationToken ct = default);
}

/// <summary>
/// One SMS gateway implementation. Registered in DI as one of many; <see cref="ISmsProviderRegistry"/>
/// picks the one whose <see cref="Key"/> matches the website's configured provider — the same shape
/// as <see cref="IFileStorageProvider"/>, so adding a gateway is a new class and nothing else.
/// </summary>
public interface ISmsProvider
{
    /// <summary>Stable provider key stored in <c>WebsiteSmsSetting.Provider</c>, e.g. <c>Kavenegar</c>.</summary>
    string Key { get; }

    /// <summary>Human-readable name for the admin gateway picker.</summary>
    string DisplayName { get; }

    /// <summary>True when this gateway is intended for Iranian numbers/panels (used only for grouping in the UI).</summary>
    bool IsIranian { get; }

    /// <summary>False when the settings JSON is missing something the gateway cannot send without.</summary>
    bool IsConfigured(SmsSettingContext ctx);

    /// <summary>Delivers one message. Throws nothing on a gateway error — returns the failure instead.</summary>
    Task<SmsSendResult> SendAsync(SmsSettingContext ctx, string countryCode, string cellphone, string message,
        CancellationToken ct = default);
}

/// <summary>Resolves the <see cref="ISmsProvider"/> registered for a provider key.</summary>
public interface ISmsProviderRegistry
{
    /// <summary>The provider for <paramref name="key"/>, or null when none is registered.</summary>
    ISmsProvider? Find(string? key);

    /// <summary>Every registered gateway, for the admin picker.</summary>
    IReadOnlyList<ISmsProvider> All { get; }
}

/// <summary>Admin-side CRUD over a website's registered SMS gateways.</summary>
public interface ISmsSettingService
{
    Task<IReadOnlyList<SmsSettingInfo>> GetForWebsiteAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Active gateways for a website, lowest sort order first.</summary>
    Task<IReadOnlyList<SmsSettingInfo>> GetActiveForWebsiteAsync(int websiteId, CancellationToken ct = default);

    Task<SmsSettingInfo?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(SmsSettingInput input, CancellationToken ct = default);

    Task UpdateAsync(int id, SmsSettingInput input, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Sends a probe message so the admin can confirm credentials before going live.</summary>
    Task<SmsSendResult> TestAsync(int id, string countryCode, string cellphone, CancellationToken ct = default);
}
