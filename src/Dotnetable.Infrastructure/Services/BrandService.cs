using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class BrandService : IBrandService
{
    private readonly AppDbContext _context;

    public BrandService(AppDbContext context) => _context = context;

    public async Task<List<Brand>> GetAllAsync(int? websiteId, CancellationToken ct = default)
    {
        var q = _context.Brands.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(b => b.WebsiteID == wid);
        return await q.OrderBy(b => b.Name).ToListAsync(ct);
    }

    public async Task<Brand?> GetByIdAsync(int brandId, CancellationToken ct = default) =>
        await _context.Brands.FindAsync([brandId], ct);

    public async Task<Brand> CreateAsync(Brand brand, CancellationToken ct = default)
    {
        _context.Brands.Add(brand);
        await _context.SaveChangesAsync(ct);
        return brand;
    }

    public async Task UpdateAsync(Brand brand, CancellationToken ct = default)
    {
        _context.Brands.Update(brand);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int brandId, CancellationToken ct = default)
    {
        var brand = await _context.Brands
            .Include(b => b.BrandTranslations)
            .Include(b => b.Products)
            .FirstOrDefaultAsync(b => b.BrandID == brandId, ct);
        if (brand is null) return;

        foreach (var product in brand.Products)
            product.BrandID = null;
        _context.BrandTranslations.RemoveRange(brand.BrandTranslations);
        _context.Brands.Remove(brand);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<BrandTranslation>> GetTranslationsAsync(int brandId, CancellationToken ct = default) =>
        await _context.BrandTranslations.AsNoTracking()
            .Where(t => t.BrandID == brandId)
            .ToListAsync(ct);

    public async Task SetTranslationsAsync(int brandId, IReadOnlyDictionary<string, (string Name, string Slug)> byLanguage, CancellationToken ct = default)
    {
        var existing = await _context.BrandTranslations
            .Where(t => t.BrandID == brandId)
            .ToListAsync(ct);

        foreach (var (languageCode, value) in byLanguage)
        {
            var current = existing.FirstOrDefault(t =>
                string.Equals(t.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(value.Name))
            {
                if (current is not null) _context.BrandTranslations.Remove(current);
                continue;
            }

            var slug = string.IsNullOrWhiteSpace(value.Slug) ? value.Name.Trim() : value.Slug.Trim();
            if (current is null)
                _context.BrandTranslations.Add(new BrandTranslation
                {
                    BrandID = brandId,
                    LanguageCode = languageCode,
                    Name = value.Name.Trim(),
                    Slug = slug,
                });
            else
            {
                current.Name = value.Name.Trim();
                current.Slug = slug;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<BrandDto>> GetActiveAsync(int websiteId, string? languageCode = null, CancellationToken ct = default)
    {
        var brands = await _context.Brands.AsNoTracking()
            .Where(b => b.WebsiteID == websiteId && b.IsActive)
            .Include(b => b.BrandTranslations)
            .Include(b => b.LogoFile)
            .OrderBy(b => b.Name)
            .ToListAsync(ct);

        return brands.Select(b =>
        {
            var name = b.Name; var slug = b.Slug;
            if (!string.IsNullOrWhiteSpace(languageCode))
            {
                var t = b.BrandTranslations.FirstOrDefault(x =>
                    string.Equals(x.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));
                if (t is not null && !string.IsNullOrWhiteSpace(t.Name)) { name = t.Name; slug = string.IsNullOrWhiteSpace(t.Slug) ? b.Slug : t.Slug; }
            }
            return new BrandDto
            {
                BrandID = b.BrandID, Name = name, Slug = slug,
                LogoUrl = b.LogoFile?.ThumbnailCDN ?? b.LogoFile?.CNDUrl,
            };
        }).ToList();
    }
}
