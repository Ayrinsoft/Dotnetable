using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class WebsiteService : IWebsiteService
{
    private readonly AppDbContext _context;

    public WebsiteService(AppDbContext context) => _context = context;

    public async Task<Website?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.Websites.FindAsync([id], ct);

    public async Task<Website?> GetByAddressAsync(string websiteAddress, CancellationToken ct = default) =>
        await _context.Websites.FirstOrDefaultAsync(w => w.WebsiteAddress == websiteAddress, ct);

    public async Task<Website?> GetByAuthCodeAsync(Guid authCode, CancellationToken ct = default) =>
        await _context.Websites.FirstOrDefaultAsync(w => w.AuthCode == authCode, ct);

    public async Task<IEnumerable<Website>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Websites.OrderBy(w => w.WebsiteID).ToListAsync(ct);

    public async Task<PagedResult<Website>> GetPagedAsync(GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Websites.AsNoTracking();

        if (query.GetSearch("TradeName") is string trade)
            q = q.Where(w => w.TradeName.Contains(trade));
        if (query.GetSearch("BrandName") is string brand)
            q = q.Where(w => w.BrandName.Contains(brand));
        if (query.GetSearch("WebsiteAddress") is string address)
            q = q.Where(w => w.WebsiteAddress.Contains(address));
        if (query.GetSearch("Manager") is string manager)
            q = q.Where(w => w.Manager.Contains(manager));
        if (query.GetSearch("Active") is string active && bool.TryParse(active, out var isActive))
            q = q.Where(w => w.Active == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Website.WebsiteID))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Website> { Items = items, TotalCount = total };
    }

    public async Task SetActiveAsync(int id, bool active, CancellationToken ct = default)
    {
        await _context.Websites.Where(w => w.WebsiteID == id)
            .ExecuteUpdateAsync(s => s.SetProperty(w => w.Active, active), ct);
    }

    public async Task<Website> CreateAsync(Website website, CancellationToken ct = default)
    {
        _context.Websites.Add(website);
        await _context.SaveChangesAsync(ct);
        return website;
    }

    public async Task UpdateAsync(Website website, CancellationToken ct = default)
    {
        _context.Websites.Update(website);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var website = await _context.Websites.FindAsync([id], ct);
        if (website is null) return;
        _context.Websites.Remove(website);
        await _context.SaveChangesAsync(ct);
    }
}
