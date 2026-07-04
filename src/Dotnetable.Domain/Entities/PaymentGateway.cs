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

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Website Website { get; set; } = null!;
}
