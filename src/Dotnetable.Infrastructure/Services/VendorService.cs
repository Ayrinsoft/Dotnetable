using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class VendorService : IVendorService
{
    private readonly AppDbContext _context;

    public VendorService(AppDbContext context) => _context = context;

    public async Task<List<Vendor>> GetAllAsync(int? websiteId, CancellationToken ct = default)
    {
        var q = _context.Vendors.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(v => v.WebsiteID == wid);
        return await q.OrderBy(v => v.Name).ToListAsync(ct);
    }

    public async Task<PagedResult<Vendor>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Vendors.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(v => v.WebsiteID == wid);

        if (query.GetSearch(nameof(Vendor.Name)) is string name)
            q = q.Where(v => v.Name.Contains(name));
        if (query.GetSearch(nameof(Vendor.Slug)) is string slug)
            q = q.Where(v => v.Slug.Contains(slug));
        if (query.GetSearch(nameof(Vendor.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(v => v.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Vendor.Name))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Vendor> { Items = items, TotalCount = total };
    }

    public async Task<Vendor?> GetByIdAsync(int vendorId, CancellationToken ct = default) =>
        await _context.Vendors.FindAsync([vendorId], ct);

    public async Task<Vendor> CreateAsync(Vendor vendor, CancellationToken ct = default)
    {
        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync(ct);
        return vendor;
    }

    public async Task UpdateAsync(Vendor vendor, CancellationToken ct = default)
    {
        _context.Vendors.Update(vendor);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int vendorId, CancellationToken ct = default)
    {
        var vendor = await _context.Vendors
            .Include(v => v.VendorTranslations)
            .Include(v => v.VendorProducts)
            .FirstOrDefaultAsync(v => v.VendorID == vendorId, ct);
        if (vendor is null) return;

        _context.VendorProducts.RemoveRange(vendor.VendorProducts);
        _context.VendorTranslations.RemoveRange(vendor.VendorTranslations);
        _context.Vendors.Remove(vendor);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<VendorTranslation>> GetTranslationsAsync(int vendorId, CancellationToken ct = default) =>
        await _context.VendorTranslations.AsNoTracking()
            .Where(t => t.VendorID == vendorId)
            .ToListAsync(ct);

    public async Task SetTranslationsAsync(int vendorId, IReadOnlyDictionary<string, string> nameByLanguage, CancellationToken ct = default)
    {
        var existing = await _context.VendorTranslations
            .Where(t => t.VendorID == vendorId)
            .ToListAsync(ct);

        foreach (var (languageCode, name) in nameByLanguage)
        {
            var current = existing.FirstOrDefault(t =>
                string.Equals(t.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(name))
            {
                if (current is not null) _context.VendorTranslations.Remove(current);
                continue;
            }

            if (current is null)
                _context.VendorTranslations.Add(new VendorTranslation
                {
                    VendorID = vendorId,
                    LanguageCode = languageCode,
                    Name = name.Trim(),
                });
            else
                current.Name = name.Trim();
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<VendorDto>> GetActiveAsync(int websiteId, string? languageCode = null, CancellationToken ct = default)
    {
        var vendors = await _context.Vendors.AsNoTracking()
            .Where(v => v.WebsiteID == websiteId && v.IsActive)
            .Include(v => v.VendorTranslations)
            .Include(v => v.LogoFile)
            .OrderBy(v => v.Name)
            .ToListAsync(ct);

        return vendors.Select(v =>
        {
            var name = v.Name;
            if (!string.IsNullOrWhiteSpace(languageCode))
            {
                var t = v.VendorTranslations.FirstOrDefault(x =>
                    string.Equals(x.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));
                if (t is not null && !string.IsNullOrWhiteSpace(t.Name)) name = t.Name;
            }
            return new VendorDto
            {
                VendorID = v.VendorID, Name = name, Slug = v.Slug, Rating = v.Rating,
                LogoUrl = v.LogoFile?.ThumbnailCDN ?? v.LogoFile?.CNDUrl,
            };
        }).ToList();
    }
}
