using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class StockMovementService : IStockMovementService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public StockMovementService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<PagedResult<StockMovement>> GetPagedAsync(int websiteId, GridQuery query, int? variantId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.StockMovements.AsNoTracking()
            .Include(m => m.Supplier)
            .Include(m => m.ProductVariant).ThenInclude(v => v.Product)
            .Where(m => m.WebsiteID == websiteId);

        if (variantId is int vid)
            q = q.Where(m => m.ProductVariantID == vid);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(StockMovement.StockMovementID), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<StockMovement> { Items = items, TotalCount = total };
    }
}
