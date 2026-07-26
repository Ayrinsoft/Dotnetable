using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
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
        if (query.GetSearch(nameof(Vendor.VendorType)) is string typeRaw && byte.TryParse(typeRaw, out var vType))
            q = q.Where(v => v.VendorType == vType);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Vendor.Name))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Vendor> { Items = items, TotalCount = total };
    }

    public async Task<Vendor?> GetByIdAsync(int vendorId, CancellationToken ct = default) =>
        await _context.Vendors.FindAsync([vendorId], ct);

    public async Task<Vendor?> GetByMemberIdAsync(int memberId, CancellationToken ct = default) =>
        await _context.Vendors.AsNoTracking()
            .FirstOrDefaultAsync(v => v.MemberID == memberId && v.VendorType == (byte)VendorType.Member, ct);

    public async Task<Vendor> CreateAsync(Vendor vendor, CancellationToken ct = default)
    {
        NormalizeAndValidate(vendor);
        await EnsureLinksValidAsync(vendor, ct);
        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync(ct);
        return vendor;
    }

    public async Task UpdateAsync(Vendor vendor, CancellationToken ct = default)
    {
        NormalizeAndValidate(vendor);
        await EnsureLinksValidAsync(vendor, ct);
        _context.Vendors.Update(vendor);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int vendorId, CancellationToken ct = default)
    {
        var vendor = await _context.Vendors
            .Include(v => v.VendorTranslations)
            .Include(v => v.VendorProducts)
            .Include(v => v.VendorCreditTransactions)
            .FirstOrDefaultAsync(v => v.VendorID == vendorId, ct);
        if (vendor is null) return;

        _context.VendorCreditTransactions.RemoveRange(vendor.VendorCreditTransactions);
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

    public async Task<List<Vendor>> GetActiveSiteLinksAsync(int hostWebsiteId, CancellationToken ct = default)
    {
        var vendors = await _context.Vendors.AsNoTracking()
            .Where(v => v.WebsiteID == hostWebsiteId
                        && v.IsActive
                        && v.VendorType == (byte)VendorType.Site
                        && v.LinkedWebsiteID != null)
            .ToListAsync(ct);
        return vendors.Where(CanExposeCatalog).ToList();
    }

    public bool CanExposeCatalog(Vendor vendor)
    {
        if (!vendor.IsActive) return false;
        if (vendor.VendorType != (byte)VendorType.Site) return false;
        if (vendor.LinkedWebsiteID is null or <= 0) return false;
        // Immediate settlement: always expose. Credit mode: need remaining credit.
        if (vendor.SettlementMode == 0) return true;
        var available = vendor.AvailableCredit != 0 || vendor.AvailableCreditUsd == 0
            ? vendor.AvailableCredit
            : vendor.AvailableCreditUsd;
        return available > 0;
    }

    private static void NormalizeAndValidate(Vendor vendor)
    {
        vendor.Name = vendor.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(vendor.Slug))
            vendor.Slug = vendor.Name;
        vendor.Slug = vendor.Slug.Trim();

        // Site-currency credit fields are authority; keep USD dual in sync when only one side is set.
        if (vendor.CreditLimit is decimal cl && cl > 0 && (vendor.CreditLimitUsd is null or <= 0))
            vendor.CreditLimitUsd = cl;
        if (vendor.CreditLimitUsd is decimal clu && clu > 0 && (vendor.CreditLimit is null or <= 0))
            vendor.CreditLimit = clu;
        if (vendor.AvailableCredit == 0 && vendor.AvailableCreditUsd != 0)
            vendor.AvailableCredit = vendor.AvailableCreditUsd;
        if (vendor.AvailableCreditUsd == 0 && vendor.AvailableCredit != 0)
            vendor.AvailableCreditUsd = vendor.AvailableCredit;

        switch ((VendorType)vendor.VendorType)
        {
            case VendorType.Display:
                vendor.MemberID = null;
                vendor.LinkedWebsiteID = null;
                break;
            case VendorType.Member:
                vendor.LinkedWebsiteID = null;
                break;
            case VendorType.Site:
                vendor.MemberID = null;
                break;
        }

        if (vendor.SettlementMode != 1)
        {
            vendor.CreditDays = null;
            // keep credit duals for site links even in immediate mode if desired
        }
    }

    private async Task EnsureLinksValidAsync(Vendor vendor, CancellationToken ct)
    {
        if (vendor.VendorType == (byte)VendorType.Member)
        {
            if (vendor.MemberID is not int mid || mid <= 0)
                throw new InvalidOperationException("Member-managed vendors require a MemberID.");

            var member = await _context.Members.AsNoTracking()
                .FirstOrDefaultAsync(m => m.MemberID == mid, ct)
                ?? throw new InvalidOperationException("Selected member was not found.");

            if (member.WebsiteID != vendor.WebsiteID)
                throw new InvalidOperationException("Vendor member must belong to the same website.");

            var taken = await _context.Vendors.AsNoTracking()
                .AnyAsync(v => v.MemberID == mid && v.VendorID != vendor.VendorID, ct);
            if (taken)
                throw new InvalidOperationException("That member is already linked to another vendor.");
        }

        if (vendor.VendorType == (byte)VendorType.Site)
        {
            if (vendor.LinkedWebsiteID is not int lid || lid <= 0)
                throw new InvalidOperationException("Site-linked vendors require a LinkedWebsiteID.");
            if (lid == vendor.WebsiteID)
                throw new InvalidOperationException("A website cannot be linked to itself as a vendor.");

            var exists = await _context.Websites.AsNoTracking().AnyAsync(w => w.WebsiteID == lid && w.Active, ct);
            if (!exists)
                throw new InvalidOperationException("Linked website was not found or is inactive.");

            var dup = await _context.Vendors.AsNoTracking()
                .AnyAsync(v => v.WebsiteID == vendor.WebsiteID
                               && v.LinkedWebsiteID == lid
                               && v.VendorID != vendor.VendorID, ct);
            if (dup)
                throw new InvalidOperationException("That website is already linked as a vendor on this host.");
        }
    }
}
