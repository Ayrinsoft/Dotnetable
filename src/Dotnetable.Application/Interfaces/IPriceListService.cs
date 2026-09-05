using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Published price lists: custom rows (site currency by default, optional USD follow)
/// or live catalog products. Admin methods work on entities; public methods return display prices.
/// </summary>
public interface IPriceListService
{
    Task<List<PriceList>> GetAllAsync(int? websiteId, CancellationToken ct = default);
    Task<PagedResult<PriceList>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);
    Task<PriceList?> GetAsync(int priceListId, CancellationToken ct = default);
    Task<PriceList> CreateAsync(PriceList list, CancellationToken ct = default);
    Task UpdateAsync(PriceList list, CancellationToken ct = default);
    Task DeleteAsync(int priceListId, CancellationToken ct = default);

    Task<List<PriceListItem>> GetItemsAsync(int priceListId, CancellationToken ct = default);
    Task<PriceListItem?> GetItemAsync(int itemId, CancellationToken ct = default);
    Task<PriceListItem> CreateItemAsync(PriceListItem item, CancellationToken ct = default);
    Task UpdateItemAsync(PriceListItem item, CancellationToken ct = default);
    Task DeleteItemAsync(int itemId, CancellationToken ct = default);

    Task<PriceListRateSnapshot> GetRateSnapshotAsync(int websiteId, CancellationToken ct = default);
    Task<PriceListRateSnapshot?> UpdateUsdRateAsync(int websiteId, decimal usdToCurrency, CancellationToken ct = default);

    Task<IReadOnlyList<PriceListSummaryDto>> GetPublishedAsync(int websiteId, CancellationToken ct = default);
    Task<PriceListDetailDto?> GetPublishedBySlugAsync(int websiteId, string slug, CancellationToken ct = default);
}
