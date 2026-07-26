using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class DigitalDeliveryService : IDigitalDeliveryService
{
    private readonly AppDbContext _context;

    public DigitalDeliveryService(AppDbContext context) => _context = context;

    public async Task GrantForOrderAsync(int orderId, CancellationToken ct = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(o => o.OrderID == orderId, ct);
        if (order is null) return;

        if (order.Status < (byte)OrderStatus.Paid || order.Status is (byte)OrderStatus.Cancelled or (byte)OrderStatus.Refunded)
            return;

        var existingItemIds = await _context.OrderDigitalAssets.AsNoTracking()
            .Where(a => a.OrderID == orderId)
            .Select(a => a.OrderItemID)
            .ToListAsync(ct);
        var existing = existingItemIds.ToHashSet();

        var now = DateTime.UtcNow;
        foreach (var item in order.OrderItems)
        {
            if (existing.Contains(item.OrderItemID)) continue;

            var product = item.ProductVariant?.Product;
            if (product is null) continue;
            if (product.ProductType == (byte)ProductType.Physical || product.RequiresShipping)
                continue;

            // Only URL/code fields — never a FileRecord.
            if (string.IsNullOrWhiteSpace(product.DigitalDownloadUrl)
                && string.IsNullOrWhiteSpace(product.DigitalServiceUrl)
                && string.IsNullOrWhiteSpace(product.DigitalDeliveryNote))
                continue;

            _context.OrderDigitalAssets.Add(new OrderDigitalAsset
            {
                WebsiteID = order.WebsiteID,
                WebsiteClientID = order.WebsiteClientID,
                OrderID = order.OrderID,
                OrderItemID = item.OrderItemID,
                ProductID = product.ProductID,
                ProductType = product.ProductType,
                TitleSnapshot = item.TitleSnapshot,
                DigitalDownloadUrl = NullIfWhiteSpace(product.DigitalDownloadUrl),
                DigitalServiceUrl = NullIfWhiteSpace(product.DigitalServiceUrl),
                DigitalDeliveryNote = NullIfWhiteSpace(product.DigitalDeliveryNote),
                IsActive = true,
                GrantedAt = now,
            });
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<DigitalLibraryItemDto>> GetClientLibraryAsync(
        int websiteId, int clientId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.OrderDigitalAssets.AsNoTracking()
            .Where(a => a.WebsiteID == websiteId && a.WebsiteClientID == clientId && a.IsActive);

        if (query.GetSearch("Title") is string title)
            q = q.Where(a => a.TitleSnapshot.Contains(title));

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(a => a.GrantedAt)
            .Skip(query.Skip).Take(query.Take)
            .Select(a => new
            {
                Asset = a,
                OrderNumber = a.Order.OrderNumber,
                AccessCount = a.DigitalAccessLogs.Count,
                LastAccess = a.DigitalAccessLogs.Max(l => (DateTime?)l.AccessedAt),
            })
            .ToListAsync(ct);

        var dtos = items.Select(x => ToListItem(x.Asset, x.OrderNumber, x.AccessCount, x.LastAccess)).ToList();
        return new PagedResult<DigitalLibraryItemDto> { Items = dtos, TotalCount = total };
    }

    public async Task<IReadOnlyList<DigitalLibraryItemDto>> GetByOrderAsync(
        int websiteId, int clientId, int orderId, CancellationToken ct = default)
    {
        var items = await _context.OrderDigitalAssets.AsNoTracking()
            .Where(a => a.WebsiteID == websiteId && a.WebsiteClientID == clientId
                        && a.OrderID == orderId && a.IsActive)
            .OrderBy(a => a.OrderDigitalAssetID)
            .Select(a => new
            {
                Asset = a,
                OrderNumber = a.Order.OrderNumber,
                AccessCount = a.DigitalAccessLogs.Count,
                LastAccess = a.DigitalAccessLogs.Max(l => (DateTime?)l.AccessedAt),
            })
            .ToListAsync(ct);

        return items.Select(x => ToListItem(x.Asset, x.OrderNumber, x.AccessCount, x.LastAccess)).ToList();
    }

    public async Task<DigitalLibraryDetailDto?> GetDetailAsync(
        int websiteId, int clientId, int orderDigitalAssetId, CancellationToken ct = default)
    {
        var asset = await _context.OrderDigitalAssets.AsNoTracking()
            .Include(a => a.Order)
            .FirstOrDefaultAsync(a =>
                a.OrderDigitalAssetID == orderDigitalAssetId
                && a.WebsiteID == websiteId
                && a.WebsiteClientID == clientId
                && a.IsActive, ct);
        if (asset is null) return null;

        var recent = await _context.DigitalAccessLogs.AsNoTracking()
            .Where(l => l.OrderDigitalAssetID == asset.OrderDigitalAssetID)
            .OrderByDescending(l => l.AccessedAt)
            .Take(20)
            .ToListAsync(ct);

        var accessCount = await _context.DigitalAccessLogs.AsNoTracking()
            .CountAsync(l => l.OrderDigitalAssetID == asset.OrderDigitalAssetID, ct);

        return new DigitalLibraryDetailDto
        {
            OrderDigitalAssetID = asset.OrderDigitalAssetID,
            OrderID = asset.OrderID,
            OrderNumber = asset.Order.OrderNumber,
            OrderItemID = asset.OrderItemID,
            ProductID = asset.ProductID,
            ProductType = asset.ProductType,
            Title = asset.TitleSnapshot,
            HasDownloadUrl = !string.IsNullOrWhiteSpace(asset.DigitalDownloadUrl),
            HasServiceUrl = !string.IsNullOrWhiteSpace(asset.DigitalServiceUrl),
            HasDeliveryNote = !string.IsNullOrWhiteSpace(asset.DigitalDeliveryNote),
            GrantedAt = asset.GrantedAt,
            AccessCount = accessCount,
            LastAccessedAt = recent.FirstOrDefault()?.AccessedAt,
            DigitalDeliveryNote = asset.DigitalDeliveryNote,
            DigitalServiceUrl = asset.DigitalServiceUrl,
            DigitalDownloadUrl = asset.DigitalDownloadUrl,
            RecentAccess = recent.Select(l => new DigitalAccessLogDto
            {
                DigitalAccessLogID = l.DigitalAccessLogID,
                AccessType = l.AccessType,
                AccessTypeName = ((DigitalAccessType)l.AccessType).ToString(),
                AccessedAt = l.AccessedAt,
                IpAddress = l.IpAddress,
            }).ToList(),
        };
    }

    public async Task<(bool Success, string? Error)> LogAccessAsync(
        int websiteId, int clientId, int orderDigitalAssetId, DigitalAccessType accessType,
        string? ipAddress, string? userAgent, CancellationToken ct = default)
    {
        var asset = await _context.OrderDigitalAssets
            .FirstOrDefaultAsync(a =>
                a.OrderDigitalAssetID == orderDigitalAssetId
                && a.WebsiteID == websiteId
                && a.WebsiteClientID == clientId
                && a.IsActive, ct);
        if (asset is null) return (false, "Digital item not found.");

        await WriteLogAsync(asset, accessType, ipAddress, userAgent, ct);
        return (true, null);
    }

    public async Task<DigitalDownloadResult> DownloadAsync(
        int websiteId, int clientId, int orderDigitalAssetId,
        string? ipAddress, string? userAgent, CancellationToken ct = default)
    {
        var asset = await _context.OrderDigitalAssets
            .FirstOrDefaultAsync(a =>
                a.OrderDigitalAssetID == orderDigitalAssetId
                && a.WebsiteID == websiteId
                && a.WebsiteClientID == clientId
                && a.IsActive, ct);
        if (asset is null) return DigitalDownloadResult.Fail("Digital item not found.");
        if (string.IsNullOrWhiteSpace(asset.DigitalDownloadUrl))
            return DigitalDownloadResult.Fail("No download link for this item.");

        await WriteLogAsync(asset, DigitalAccessType.Download, ipAddress, userAgent, ct);
        return DigitalDownloadResult.Ok(asset.DigitalDownloadUrl.Trim());
    }

    private async Task WriteLogAsync(
        OrderDigitalAsset asset, DigitalAccessType accessType,
        string? ipAddress, string? userAgent, CancellationToken ct)
    {
        _context.DigitalAccessLogs.Add(new DigitalAccessLog
        {
            OrderDigitalAssetID = asset.OrderDigitalAssetID,
            WebsiteClientID = asset.WebsiteClientID,
            WebsiteID = asset.WebsiteID,
            AccessType = (byte)accessType,
            IpAddress = Truncate(ipAddress, 64),
            UserAgent = Truncate(userAgent, 500),
            AccessedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync(ct);
    }

    private static DigitalLibraryItemDto ToListItem(
        OrderDigitalAsset a, string orderNumber, int accessCount, DateTime? lastAccess) => new()
    {
        OrderDigitalAssetID = a.OrderDigitalAssetID,
        OrderID = a.OrderID,
        OrderNumber = orderNumber,
        OrderItemID = a.OrderItemID,
        ProductID = a.ProductID,
        ProductType = a.ProductType,
        Title = a.TitleSnapshot,
        HasDownloadUrl = !string.IsNullOrWhiteSpace(a.DigitalDownloadUrl),
        HasServiceUrl = !string.IsNullOrWhiteSpace(a.DigitalServiceUrl),
        HasDeliveryNote = !string.IsNullOrWhiteSpace(a.DigitalDeliveryNote),
        GrantedAt = a.GrantedAt,
        AccessCount = accessCount,
        LastAccessedAt = lastAccess,
    };

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Truncate(string? value, int max) =>
        string.IsNullOrEmpty(value) ? value : (value.Length <= max ? value : value[..max]);
}
