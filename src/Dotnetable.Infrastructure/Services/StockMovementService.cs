using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class StockMovementService : IStockMovementService
{
    private readonly AppDbContext _context;

    public StockMovementService(AppDbContext context) => _context = context;

    public async Task<PagedResult<StockMovement>> GetPagedAsync(int websiteId, GridQuery query, int? variantId, CancellationToken ct = default)
    {
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
