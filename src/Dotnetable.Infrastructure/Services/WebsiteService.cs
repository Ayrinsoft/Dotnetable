using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class WebsiteService : IWebsiteService
{
    private readonly AppDbContext _context;

    public WebsiteService(AppDbContext context) => _context = context;

    public async Task<Website?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.Websites.FindAsync([id], ct);

    public async Task<Website?> GetByDomainAsync(string domain, CancellationToken ct = default) =>
        await _context.Websites.FirstOrDefaultAsync(w => w.Domain == domain, ct);

    public async Task<IEnumerable<Website>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Websites.OrderBy(w => w.Name).ToListAsync(ct);

    public async Task<Website> CreateAsync(Website website, CancellationToken ct = default)
    {
        _context.Websites.Add(website);
        await _context.SaveChangesAsync(ct);
        return website;
    }

    public async Task UpdateAsync(Website website, CancellationToken ct = default)
    {
        website.UpdatedAt = DateTime.UtcNow;
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
