using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class WarrantyService : IWarrantyService
{
    private readonly AppDbContext _context;

    public WarrantyService(AppDbContext context) => _context = context;

    public async Task<List<Warranty>> GetAllAsync(int? websiteId, bool activeOnly = false, CancellationToken ct = default)
    {
        var q = _context.Warranties.AsNoTracking().AsQueryable();
        if (websiteId is int wid)
            q = q.Where(w => w.WebsiteID == wid);
        if (activeOnly)
            q = q.Where(w => w.IsActive);
        return await q.OrderBy(w => w.SortOrder).ThenBy(w => w.Title).ToListAsync(ct);
    }

    public async Task<PagedResult<Warranty>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Warranties.AsNoTracking().AsQueryable();
        if (websiteId is int wid)
            q = q.Where(w => w.WebsiteID == wid);

        if (query.GetSearch(nameof(Warranty.Title)) is string title)
            q = q.Where(w => w.Title.Contains(title));
        if (query.GetSearch(nameof(Warranty.ProviderName)) is string provider)
            q = q.Where(w => w.ProviderName != null && w.ProviderName.Contains(provider));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Warranty.SortOrder))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Warranty> { Items = items, TotalCount = total };
    }

    public async Task<Warranty?> GetByIdAsync(int warrantyId, CancellationToken ct = default) =>
        await _context.Warranties.FindAsync([warrantyId], ct);

    public async Task<Warranty> CreateAsync(Warranty warranty, CancellationToken ct = default)
    {
        _context.Warranties.Add(warranty);
        await _context.SaveChangesAsync(ct);
        return warranty;
    }

    public async Task UpdateAsync(Warranty warranty, CancellationToken ct = default)
    {
        _context.Warranties.Update(warranty);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int warrantyId, CancellationToken ct = default)
    {
        var warranty = await _context.Warranties
            .Include(w => w.WarrantyTranslations)
            .Include(w => w.ProductWarranties)
            .FirstOrDefaultAsync(w => w.WarrantyID == warrantyId, ct);
        if (warranty is null) return;

        // Detach product links so products keep a free-text copy of the title if empty.
        foreach (var pw in warranty.ProductWarranties)
        {
            if (string.IsNullOrWhiteSpace(pw.CustomTitle))
                pw.CustomTitle = warranty.Title;
            if (string.IsNullOrWhiteSpace(pw.CustomDescription) && !string.IsNullOrWhiteSpace(warranty.Description))
                pw.CustomDescription = warranty.Description;
            pw.WarrantyID = null;
        }

        _context.WarrantyTranslations.RemoveRange(warranty.WarrantyTranslations);
        _context.Warranties.Remove(warranty);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<WarrantyTranslation>> GetTranslationsAsync(int warrantyId, CancellationToken ct = default) =>
        await _context.WarrantyTranslations.AsNoTracking()
            .Where(t => t.WarrantyID == warrantyId)
            .ToListAsync(ct);

    public async Task SetTranslationsAsync(
        int warrantyId,
        IReadOnlyDictionary<string, (string Title, string? Description, string? ProviderName)> byLanguage,
        CancellationToken ct = default)
    {
        var existing = await _context.WarrantyTranslations
            .Where(t => t.WarrantyID == warrantyId)
            .ToListAsync(ct);

        foreach (var (languageCode, value) in byLanguage)
        {
            var current = existing.FirstOrDefault(t =>
                string.Equals(t.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(value.Title))
            {
                if (current is not null) _context.WarrantyTranslations.Remove(current);
                continue;
            }

            if (current is null)
                _context.WarrantyTranslations.Add(new WarrantyTranslation
                {
                    WarrantyID = warrantyId,
                    LanguageCode = languageCode,
                    Title = value.Title.Trim(),
                    Description = value.Description,
                    ProviderName = value.ProviderName,
                });
            else
            {
                current.Title = value.Title.Trim();
                current.Description = value.Description;
                current.ProviderName = value.ProviderName;
            }
        }

        await _context.SaveChangesAsync(ct);
    }
}
