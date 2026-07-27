namespace Dotnetable.Application.DTOs;

/// <summary>Checkout tax result for a single website's rates and tax settings.</summary>
public sealed class TaxComputationResult
{
    public decimal TaxAmount { get; init; }

    public bool PricesIncludeTax { get; init; }

    public bool TaxEnabled { get; init; }

    /// <summary>JSON array of applied lines: [{code,title,kind,rate,amount}].</summary>
    public string? BreakdownJson { get; init; }

    public IReadOnlyList<TaxLineDto> Lines { get; init; } = Array.Empty<TaxLineDto>();
}

public sealed class TaxLineDto
{
    public string? Code { get; init; }
    public string Title { get; init; } = "";
    public byte TaxKind { get; init; }
    public decimal Rate { get; init; }
    public decimal Amount { get; init; }
    public bool AppliedToShipping { get; init; }
}
