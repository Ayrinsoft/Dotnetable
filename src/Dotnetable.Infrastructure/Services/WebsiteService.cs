using Dotnetable.Application;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Short-lived contexts from the factory (not the circuit-scoped one). Blazor Server runs
/// layout + page OnInitializedAsync in parallel on one scope; concurrent queries on a shared
/// DbContext throw "A second operation was started on this context instance...".
/// </summary>
public class WebsiteService : IWebsiteService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public WebsiteService(IDbContextFactory<AppDbContext> contextFactory) =>
        _contextFactory = contextFactory;

    public async Task<Website?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Websites.AsNoTracking().FirstOrDefaultAsync(w => w.WebsiteID == id, ct);
    }

    public async Task<Website?> GetByAddressAsync(string websiteAddress, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Websites.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebsiteAddress == websiteAddress, ct);
    }

    public async Task<Website?> GetByAuthCodeAsync(Guid authCode, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Websites.AsNoTracking()
            .FirstOrDefaultAsync(w => w.AuthCode == authCode, ct);
    }

    public async Task<IEnumerable<Website>> GetAllAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Websites.AsNoTracking().OrderBy(w => w.WebsiteID).ToListAsync(ct);
    }

    public async Task<PagedResult<Website>> GetPagedAsync(GridQuery query, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var q = context.Websites.AsNoTracking();

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
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await context.Websites.Where(w => w.WebsiteID == id)
            .ExecuteUpdateAsync(s => s.SetProperty(w => w.Active, active), ct);
    }

    public async Task<Website> CreateAsync(Website website, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        website.ProductCodePrefix = Domain.ProductCode.NormalizePrefix(website.ProductCodePrefix);
        // Dual USD is opt-in; default operational mode is single site currency.
        context.Websites.Add(website);

        var defaults = ((WebsiteType)website.WebsiteType).GetDefaultFeatures();
        var featureNow = DateTime.UtcNow;
        foreach (var featureKey in defaults)
        {
            context.WebsiteFeatures.Add(new WebsiteFeature
            {
                Website = website,
                FeatureKey = (byte)featureKey,
                Enabled = true,
                CreatedAt = featureNow,
            });
        }

        await context.SaveChangesAsync(ct);

        // Operational currency rate so the site works without multi-currency FX setup.
        if (!string.IsNullOrWhiteSpace(website.DefaultCurrencyCode)
            && !await context.CurrencyRates.AnyAsync(r => r.WebsiteID == website.WebsiteID, ct))
        {
            context.CurrencyRates.Add(new CurrencyRate
            {
                WebsiteID = website.WebsiteID,
                CurrencyCode = website.DefaultCurrencyCode,
                USDToCurrency = 1m,
                IsDefault = true,
                LastUpdate = DateTime.UtcNow,
            });
            await context.SaveChangesAsync(ct);
        }

        // Default wallet currency = site operational currency (separate multi-currency wallets later).
        if (!string.IsNullOrWhiteSpace(website.DefaultCurrencyCode)
            && !await context.WebsiteWalletCurrencies.AnyAsync(c => c.WebsiteID == website.WebsiteID, ct))
        {
            context.WebsiteWalletCurrencies.Add(new WebsiteWalletCurrency
            {
                WebsiteID = website.WebsiteID,
                CurrencyCode = website.DefaultCurrencyCode.Trim().ToUpperInvariant(),
                IsDefault = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            });
            await context.SaveChangesAsync(ct);
        }

        return website;
    }

    public async Task UpdateAsync(Website website, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        website.ProductCodePrefix = Domain.ProductCode.NormalizePrefix(website.ProductCodePrefix);
        context.Websites.Update(website);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var website = await context.Websites.FindAsync([id], ct);
        if (website is null) return;
        context.Websites.Remove(website);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<WebsiteFeature>> GetFeaturesAsync(int websiteId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.WebsiteFeatures.AsNoTracking()
            .Where(f => f.WebsiteID == websiteId).ToListAsync(ct);
    }

    public async Task SetFeatureAsync(int websiteId, WebsiteFeatureKey featureKey, bool enabled, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var existing = await context.WebsiteFeatures
            .FirstOrDefaultAsync(f => f.WebsiteID == websiteId && f.FeatureKey == (byte)featureKey, ct);

        if (existing is not null)
        {
            existing.Enabled = enabled;
        }
        else
        {
            context.WebsiteFeatures.Add(new WebsiteFeature
            {
                WebsiteID = websiteId,
                FeatureKey = (byte)featureKey,
                Enabled = enabled,
                CreatedAt = DateTime.UtcNow,
            });
        }

        await context.SaveChangesAsync(ct);
    }

    public async Task<SiteInfoDto?> GetSiteInfoAsync(int websiteId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var website = await context.Websites.AsNoTracking()
            .Include(w => w.LogoFile)
            .Include(w => w.FaveIconFile)
            .Include(w => w.WebsiteSocialLinks)
            .Include(w => w.WebsiteSeoSettings)
            .FirstOrDefaultAsync(w => w.WebsiteID == websiteId, ct);
        if (website is null) return null;

        var seo = website.WebsiteSeoSettings.FirstOrDefault();
        return new SiteInfoDto
        {
            BrandName = website.BrandName,
            TradeName = website.TradeName,
            LogoUrl = website.LogoFile?.CNDUrl,
            FavIconUrl = website.FaveIconFile?.CNDUrl,
            Email = website.Email,
            Phone = website.Mobile,
            DefaultLanguageCode = website.DefaultLanguageCode,
            DefaultMetaTitle = seo?.DefaultMetaTitle,
            DefaultMetaDescription = seo?.DefaultMetaDescription,
            TitleSeparator = string.IsNullOrWhiteSpace(seo?.TitleSeparator) ? "·" : seo.TitleSeparator,
            SocialLinks = website.WebsiteSocialLinks
                .OrderBy(s => s.WebsiteSocialLinkID)
                .Select(s => new SocialLinkDto
                {
                    Name = s.SocialName,
                    Icon = s.SocialIcon,
                    Url = s.UrlAddress,
                })
                .ToList(),
        };
    }

    public async Task<WebsiteCaptchaSetting?> GetCaptchaSettingAsync(int websiteId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.WebsiteCaptchaSettings.AsNoTracking()
            .FirstOrDefaultAsync(c => c.WebsiteID == websiteId, ct);
    }

    public async Task<string?> GetDefaultPhoneCountryCodeAsync(int websiteId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var website = await context.Websites.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebsiteID == websiteId, ct);
        if (website is null) return null;

        string? taxPrefix = null;
        if (website.TaxCountryID is int cid)
        {
            taxPrefix = await context.Countries.AsNoTracking()
                .Where(c => c.CountryID == cid)
                .Select(c => c.PhonePerfix)
                .FirstOrDefaultAsync(ct);
        }

        var prefixes = await context.Countries.AsNoTracking()
            .Select(c => c.PhonePerfix)
            .ToListAsync(ct);

        return PhoneCountryCodes.FromWebsiteMobile(website.Mobile, prefixes, taxPrefix);
    }
}
