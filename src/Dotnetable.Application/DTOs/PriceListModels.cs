using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

/// <summary>Public listing card for an active price list.</summary>
public sealed class PriceListSummaryDto
{
    public int PriceListID { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int ItemCount { get; init; }
    public DateTime LastUpdatedAt { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public PriceListSource Source { get; init; }
    public bool FollowsUsd { get; init; }
}

/// <summary>Full public price list with live display prices.</summary>
public sealed class PriceListDetailDto
{
    public int PriceListID { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime LastUpdatedAt { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public int DecimalDigits { get; init; }
    public PriceListSource Source { get; init; }
    public bool FollowsUsd { get; init; }
    public decimal UsdToCurrency { get; init; }
    public DateTime? ExchangeRateLastUpdate { get; init; }
    public IReadOnlyList<PriceListGroupDto> Groups { get; init; } = Array.Empty<PriceListGroupDto>();
}

public sealed class PriceListGroupDto
{
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<PriceListItemDto> Items { get; init; } = Array.Empty<PriceListItemDto>();
}

public sealed class PriceListItemDto
{
    public int PriceListItemID { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? GroupName { get; init; }
    public string? Specification { get; init; }
    public string? Unit { get; init; }
    public string? Sku { get; init; }
    public bool LinkToUsd { get; init; }
    public string? ProductSlug { get; init; }
    public MoneyDto Price { get; init; } = null!;
}

/// <summary>Admin snapshot of the USD rate used when a list opts into FX-linked prices.</summary>
public sealed class PriceListRateSnapshot
{
    public int? CurrencyRateID { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public decimal UsdToCurrency { get; init; }
    public DateTime? LastUpdate { get; init; }
    public int DecimalDigits { get; init; }
    public bool HasRate { get; init; }
}
