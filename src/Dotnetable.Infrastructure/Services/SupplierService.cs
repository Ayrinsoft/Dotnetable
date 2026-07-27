using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly AppDbContext _context;

    public SupplierService(AppDbContext context) => _context = context;

    public async Task<List<Supplier>> GetAllAsync(int websiteId, CancellationToken ct = default) =>
        await _context.Suppliers.AsNoTracking()
            .Where(s => s.WebsiteID == websiteId)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

    public async Task<PagedResult<Supplier>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Suppliers.AsNoTracking()
            .Include(s => s.Country)
            .Include(s => s.LinkedWebsite)
            .Where(s => s.WebsiteID == websiteId);

        if (query.GetSearch(nameof(Supplier.Name)) is string name)
            q = q.Where(s => s.Name.Contains(name) || (s.LegalName != null && s.LegalName.Contains(name)));
        if (query.GetSearch(nameof(Supplier.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(s => s.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Supplier.Name))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Supplier> { Items = items, TotalCount = total };
    }

    public async Task<Supplier?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.Suppliers
            .Include(s => s.Country)
            .Include(s => s.LinkedWebsite)
            .Include(s => s.LinkedVendor)
            .FirstOrDefaultAsync(s => s.SupplierID == id, ct);

    public async Task<Supplier> CreateAsync(Supplier supplier, CancellationToken ct = default)
    {
        Normalize(supplier);
        if (supplier.CreatedAt == default)
            supplier.CreatedAt = DateTime.UtcNow;
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(ct);
        return supplier;
    }

    public async Task UpdateAsync(Supplier supplier, CancellationToken ct = default)
    {
        var existing = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierID == supplier.SupplierID, ct);
        if (existing is null) return;

        Normalize(supplier);

        existing.Name = supplier.Name;
        existing.LegalName = supplier.LegalName;
        existing.Phone = supplier.Phone;
        existing.Email = supplier.Email;
        existing.AddressLine = supplier.AddressLine;
        existing.CityName = supplier.CityName;
        existing.PostalCode = supplier.PostalCode;
        existing.CountryID = supplier.CountryID;
        existing.TaxIdentificationNumber = supplier.TaxIdentificationNumber;
        existing.EconomicCode = supplier.EconomicCode;
        existing.VatNumber = supplier.VatNumber;
        existing.RegistrationNumber = supplier.RegistrationNumber;
        existing.IsVatRegistered = supplier.IsVatRegistered;
        existing.BankName = supplier.BankName;
        existing.BankIban = supplier.BankIban;
        existing.BankAccountNumber = supplier.BankAccountNumber;
        existing.DefaultCurrencyCode = supplier.DefaultCurrencyCode;
        existing.SupplierType = supplier.SupplierType;
        existing.LinkedWebsiteID = supplier.LinkedWebsiteID;
        existing.LinkedVendorID = supplier.LinkedVendorID;
        existing.Notes = supplier.Notes;
        existing.IsActive = supplier.IsActive;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.Suppliers.FindAsync([id], ct);
        if (entity is null) return;
        _context.Suppliers.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Ensures a LinkedWebsite supplier exists on the host for inter-site vendor settlements.
    /// Returns the supplier id for tax/settlement counterparty tracking.
    /// </summary>
    public async Task<int> EnsureLinkedWebsiteSupplierAsync(
        int hostWebsiteId, int sourceWebsiteId, string? displayName, CancellationToken ct = default)
    {
        var existing = await _context.Suppliers.FirstOrDefaultAsync(s =>
            s.WebsiteID == hostWebsiteId
            && s.SupplierType == (byte)SupplierType.LinkedWebsite
            && s.LinkedWebsiteID == sourceWebsiteId, ct);
        if (existing is not null) return existing.SupplierID;

        var source = await _context.Websites.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebsiteID == sourceWebsiteId, ct);

        var supplier = new Supplier
        {
            WebsiteID = hostWebsiteId,
            Name = displayName ?? source?.BrandName ?? source?.TradeName ?? $"Site #{sourceWebsiteId}",
            LegalName = source?.SellerLegalName ?? source?.TradeName,
            TaxIdentificationNumber = source?.SellerTaxId,
            EconomicCode = source?.SellerEconomicCode,
            VatNumber = source?.SellerVatNumber,
            RegistrationNumber = source?.SellerRegistrationNumber,
            IsVatRegistered = !string.IsNullOrWhiteSpace(source?.SellerVatNumber),
            CountryID = source?.TaxCountryID,
            DefaultCurrencyCode = source?.DefaultCurrencyCode,
            SupplierType = (byte)SupplierType.LinkedWebsite,
            LinkedWebsiteID = sourceWebsiteId,
            Email = source?.Email,
            Phone = source?.Mobile,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Notes = "Auto-created for inter-site vendor settlement / tax counterparty.",
        };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(ct);
        return supplier.SupplierID;
    }

    private static void Normalize(Supplier s)
    {
        s.Name = s.Name.Trim();
        s.LegalName = NullIfWhite(s.LegalName);
        s.Phone = NullIfWhite(s.Phone);
        s.Email = NullIfWhite(s.Email);
        s.AddressLine = NullIfWhite(s.AddressLine);
        s.CityName = NullIfWhite(s.CityName);
        s.PostalCode = NullIfWhite(s.PostalCode);
        s.TaxIdentificationNumber = NullIfWhite(s.TaxIdentificationNumber);
        s.EconomicCode = NullIfWhite(s.EconomicCode);
        s.VatNumber = NullIfWhite(s.VatNumber);
        s.RegistrationNumber = NullIfWhite(s.RegistrationNumber);
        s.BankName = NullIfWhite(s.BankName);
        s.BankIban = NullIfWhite(s.BankIban);
        s.BankAccountNumber = NullIfWhite(s.BankAccountNumber);
        s.DefaultCurrencyCode = NullIfWhite(s.DefaultCurrencyCode)?.ToUpperInvariant();
        s.Notes = NullIfWhite(s.Notes);

        if (s.SupplierType == (byte)SupplierType.External)
        {
            s.LinkedWebsiteID = null;
            s.LinkedVendorID = null;
        }
        else if (s.SupplierType == (byte)SupplierType.LinkedWebsite)
        {
            s.LinkedVendorID = null;
        }
        else if (s.SupplierType == (byte)SupplierType.LinkedVendor)
        {
            s.LinkedWebsiteID = null;
        }
    }

    private static string? NullIfWhite(string? v) =>
        string.IsNullOrWhiteSpace(v) ? null : v.Trim();
}
