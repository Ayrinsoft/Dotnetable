using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Currency
{
    public string CurrencyCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Symbol { get; set; } = null!;

    public byte DecimalDigits { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<CurrencyRate> CurrencyRates { get; set; } = new List<CurrencyRate>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual ICollection<Website> Websites { get; set; } = new List<Website>();
}
