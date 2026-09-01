using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class BrandService : IBrandService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public BrandService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<List<Brand>> GetAllAsync(int? websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.Brands.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(b => b.WebsiteID == wid);
        return await q.OrderBy(b => b.Name).ToListAsync(ct);
    }

    public async Task<PagedResult<Brand>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.Brands.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(b => b.WebsiteID == wid);

        if (query.GetSearch(nameof(Brand.Name)) is string name)
            q = q.Where(b => b.Name.Contains(name));
        if (query.GetSearch(nameof(Brand.Slug)) is string slug)
            q = q.Where(b => b.Slug.Contains(slug));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Brand.Name))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Brand> { Items = items, TotalCount = total };
    }

    public async Task<Brand?> GetByIdAsync(int brandId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.Brands.FindAsync([brandId], ct);
    }

    public async Task<Brand> CreateAsync(Brand brand, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        _context.Brands.Add(brand);
        await _context.SaveChangesAsync(ct);
        return brand;
    }

    public async Task UpdateAsync(Brand brand, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        _context.Brands.Update(brand);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int brandId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

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

    public async Task<List<BrandTranslation>> GetTranslationsAsync(int brandId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.BrandTranslations.AsNoTracking()
            .Where(t => t.BrandID == brandId)
            .ToListAsync(ct);
    }

    public async Task SetTranslationsAsync(int brandId, IReadOnlyDictionary<string, (string Name, string Slug)> byLanguage, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

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
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

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
