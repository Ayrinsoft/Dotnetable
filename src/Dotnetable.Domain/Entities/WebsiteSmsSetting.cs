using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// A per-website SMS gateway registration, mirroring <see cref="WebsiteStorageSetting"/>: the provider
/// key selects an <c>ISmsProvider</c> implementation and <see cref="SettingsJSON"/> carries that
/// provider's own credentials. The active row with the lowest <see cref="SortOrder"/> sends.
/// </summary>
public partial class WebsiteSmsSetting
{
    public int WebsiteSmsSettingID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Provider key, e.g. <c>Kavenegar</c>, <c>MelliPayamak</c>, <c>Twilio</c>. Matches <c>ISmsProvider.Key</c>.</summary>
    public string Provider { get; set; } = null!;

    /// <summary>Label shown in the admin list.</summary>
    public string Title { get; set; } = null!;

    /// <summary>Provider credentials/options JSON, parsed by the matching provider.</summary>
    public string SettingsJSON { get; set; } = "{}";

    /// <summary>Default sender line/number, when the provider needs one outside its settings.</summary>
    public string? SenderNumber { get; set; }

    public bool IsActive { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
}
