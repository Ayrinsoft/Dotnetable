using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// One WhatsApp gateway implementation — the same shape as <see cref="ISmsProvider"/>. Registered in DI
/// as one of many; <see cref="IWhatsAppProviderRegistry"/> picks the one whose <see cref="Key"/> matches
/// the website's configured provider.
/// </summary>
public interface IWhatsAppProvider
{
    /// <summary>Stable provider key stored in <c>WebsiteWhatsAppSetting.Provider</c>.</summary>
    string Key { get; }

    /// <summary>Human-readable name for the admin gateway picker.</summary>
    string DisplayName { get; }

    /// <summary>False when the settings JSON is missing something the gateway cannot send without.</summary>
    bool IsConfigured(WhatsAppSettingContext ctx);

    /// <summary>Delivers one message. Throws nothing on a gateway error — returns the failure instead.</summary>
    Task<SmsSendResult> SendAsync(WhatsAppSettingContext ctx, string countryCode, string cellphone, string message,
        CancellationToken ct = default);
}

/// <summary>Resolves the <see cref="IWhatsAppProvider"/> registered for a provider key.</summary>
public interface IWhatsAppProviderRegistry
{
    IWhatsAppProvider? Find(string? key);

    IReadOnlyList<IWhatsAppProvider> All { get; }
}

/// <summary>Admin-side CRUD over a website's registered WhatsApp gateways.</summary>
public interface IWhatsAppSettingService
{
    Task<IReadOnlyList<WhatsAppSettingInfo>> GetForWebsiteAsync(int websiteId, CancellationToken ct = default);

    /// <summary>The editable payload including the settings JSON (credentials).</summary>
    Task<WhatsAppSettingInput?> GetInputAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(WhatsAppSettingInput input, CancellationToken ct = default);

    Task UpdateAsync(int id, WhatsAppSettingInput input, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Sends a probe message so the admin can confirm credentials before going live.</summary>
    Task<SmsSendResult> TestAsync(int id, string countryCode, string cellphone, CancellationToken ct = default);
}
