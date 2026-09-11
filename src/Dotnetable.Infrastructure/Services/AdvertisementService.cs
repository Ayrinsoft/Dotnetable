using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class AdvertisementService : IAdvertisementService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public AdvertisementService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<List<Advertisement>> GetAllAsync(int? websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.Advertisements.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(a => a.WebsiteID == wid);
        return await q.OrderBy(a => a.Location).ThenBy(a => a.SortOrder).ThenBy(a => a.AdvertisementID).ToListAsync(ct);
    }

    public async Task<PagedResult<Advertisement>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.Advertisements.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(a => a.WebsiteID == wid);

        if (query.GetSearch(nameof(Advertisement.Keyword)) is string keyword)
            q = q.Where(a => a.Keyword.Contains(keyword));
        if (query.GetSearch(nameof(Advertisement.Url)) is string url)
            q = q.Where(a => a.Url.Contains(url));
        if (query.GetSearch(nameof(Advertisement.Location)) is string loc && byte.TryParse(loc, out var location))
            q = q.Where(a => a.Location == location);
        if (query.GetSearch(nameof(Advertisement.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(a => a.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Advertisement.SortOrder))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Advertisement> { Items = items, TotalCount = total };
    }

    public async Task<Advertisement?> GetByIdAsync(int advertisementId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.Advertisements.FindAsync([advertisementId], ct);
    }

    public async Task<Advertisement> CreateAsync(Advertisement advertisement, CancellationToken ct = default)
    {
        Normalize(advertisement);
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        advertisement.CreatedAt = DateTime.UtcNow;
        _context.Advertisements.Add(advertisement);
        await _context.SaveChangesAsync(ct);
        return advertisement;
    }

    public async Task UpdateAsync(Advertisement advertisement, CancellationToken ct = default)
    {
        Normalize(advertisement);
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        _context.Advertisements.Update(advertisement);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int advertisementId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var advertisement = await _context.Advertisements
            .Include(a => a.AdvertisementTranslations)
            .FirstOrDefaultAsync(a => a.AdvertisementID == advertisementId, ct);
        if (advertisement is null) return;

        _context.AdvertisementTranslations.RemoveRange(advertisement.AdvertisementTranslations);
        _context.Advertisements.Remove(advertisement);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<AdvertisementTranslation>> GetTranslationsAsync(int advertisementId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.AdvertisementTranslations.AsNoTracking()
            .Where(t => t.AdvertisementID == advertisementId)
            .ToListAsync(ct);
    }

    public async Task SetTranslationsAsync(int advertisementId, IReadOnlyDictionary<string, (string Keyword, string? Url)> byLanguage, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.AdvertisementTranslations
            .Where(t => t.AdvertisementID == advertisementId)
            .ToListAsync(ct);

        foreach (var (languageCode, value) in byLanguage)
        {
            var current = existing.FirstOrDefault(t =>
                string.Equals(t.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(value.Keyword))
            {
                if (current is not null) _context.AdvertisementTranslations.Remove(current);
                continue;
            }

            var url = string.IsNullOrWhiteSpace(value.Url) ? null : NormalizeUrl(value.Url);
            if (current is null)
                _context.AdvertisementTranslations.Add(new AdvertisementTranslation
                {
                    AdvertisementID = advertisementId,
                    LanguageCode = languageCode,
                    Keyword = value.Keyword.Trim(),
                    Url = url,
                });
            else
            {
                current.Keyword = value.Keyword.Trim();
                current.Url = url;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<AdvertisementDto>> GetByLocationAsync(int websiteId, AdvertisementLocation location, string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var ads = await _context.Advertisements.AsNoTracking()
            .Where(a => a.WebsiteID == websiteId && a.IsActive && a.Location == (byte)location)
            .Include(a => a.AdvertisementTranslations)
            .OrderBy(a => a.SortOrder)
            .ThenBy(a => a.AdvertisementID)
            .ToListAsync(ct);

        return ads.Select(a => Project(a, languageCode))
            .Where(a => !string.IsNullOrWhiteSpace(a.Keyword) && !string.IsNullOrWhiteSpace(a.Url))
            .ToList();
    }

    private static AdvertisementDto Project(Advertisement ad, string? languageCode)
    {
        var keyword = ad.Keyword;
        var url = ad.Url;
        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            var t = ad.AdvertisementTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));
            if (t is not null)
            {
                if (!string.IsNullOrWhiteSpace(t.Keyword)) keyword = t.Keyword;
                if (!string.IsNullOrWhiteSpace(t.Url)) url = t.Url;
            }
        }

        return new AdvertisementDto
        {
            AdvertisementID = ad.AdvertisementID,
            Location = (AdvertisementLocation)ad.Location,
            Keyword = keyword,
            Url = url,
            OpenInNewTab = ad.OpenInNewTab,
        };
    }

    private static void Normalize(Advertisement advertisement)
    {
        advertisement.Keyword = (advertisement.Keyword ?? string.Empty).Trim();
        advertisement.Url = NormalizeUrl(advertisement.Url);
        if (string.IsNullOrWhiteSpace(advertisement.Keyword))
            throw new ArgumentException("Keyword is required.", nameof(advertisement));
        if (advertisement.Location == 0)
            advertisement.Location = (byte)AdvertisementLocation.Header;
    }

    /// <summary>Accepts http(s) URLs and site-relative paths; rejects javascript:/data: and other schemes.</summary>
    private static string NormalizeUrl(string? url)
    {
        var trimmed = (url ?? string.Empty).Trim();
        if (trimmed.Length == 0)
            throw new ArgumentException("URL is required.");
        if (trimmed.StartsWith('/') && !trimmed.StartsWith("//", StringComparison.Ordinal))
            return trimmed;
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            return uri.ToString();
        throw new ArgumentException("URL must be http(s) or a site-relative path.");
    }
}
