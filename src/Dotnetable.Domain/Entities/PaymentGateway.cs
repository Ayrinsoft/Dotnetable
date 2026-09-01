using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class PaymentGateway
{
    public int PaymentGatewayID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public string Provider { get; set; } = null!;

    public string? MerchantID { get; set; }

    public string? ApiKey { get; set; }

    public string? ApiSecret { get; set; }

    public bool IsSandbox { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    /// <summary>Provider-specific credentials/options JSON, parsed by the matching IPaymentGatewayProvider. Preferred over the flat MerchantID/ApiKey/ApiSecret columns.</summary>
    public string? SettingsJSON { get; set; }

    /// <summary>Absolute return URL the PSP sends the payer back to. Blank falls back to the website address + /checkout/callback.</summary>
    public string? CallbackUrl { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Website Website { get; set; } = null!;
}
