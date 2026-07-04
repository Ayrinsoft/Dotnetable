using Dotnetable.Application;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class WebsiteClientAddressService : IWebsiteClientAddressService
{
    private readonly AppDbContext _context;

    public WebsiteClientAddressService(AppDbContext context) => _context = context;

    public async Task<List<WebsiteClientAddress>> GetByClientIdAsync(int clientId, CancellationToken ct = default) =>
        await _context.WebsiteClientAddresses.AsNoTracking()
            .Include(a => a.Country)
            .Include(a => a.City).ThenInclude(c => c!.State)
            .Where(a => a.WebsiteClientID == clientId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.WebsiteClientAddressID)
            .ToListAsync(ct);

    public async Task<WebsiteClientAddress?> GetByIdAsync(int addressId, int clientId, CancellationToken ct = default) =>
        await _context.WebsiteClientAddresses.AsNoTracking()
            .Include(a => a.Country)
            .Include(a => a.City).ThenInclude(c => c!.State)
            .FirstOrDefaultAsync(a => a.WebsiteClientAddressID == addressId && a.WebsiteClientID == clientId, ct);

    public async Task<AddressSaveResult> CreateAsync(WebsiteClientAddress address, CancellationToken ct = default)
    {
        var count = await _context.WebsiteClientAddresses
            .CountAsync(a => a.WebsiteClientID == address.WebsiteClientID, ct);
        if (count >= AppConstants.MaxClientAddresses)
            return AddressSaveResult.LimitReached;

        if (address.IsDefault)
            await ClearDefaultAsync(address.WebsiteClientID, ct);
        else if (count == 0)
            address.IsDefault = true; // first address is always the default

        _context.WebsiteClientAddresses.Add(address);
        await _context.SaveChangesAsync(ct);
        return AddressSaveResult.Success;
    }

    public async Task<AddressSaveResult> UpdateAsync(WebsiteClientAddress address, CancellationToken ct = default)
    {
        var existing = await _context.WebsiteClientAddresses
            .FirstOrDefaultAsync(a => a.WebsiteClientAddressID == address.WebsiteClientAddressID
                                       && a.WebsiteClientID == address.WebsiteClientID, ct);
        if (existing is null)
            return AddressSaveResult.NotFound;

        existing.Title = address.Title;
        existing.ReceiverName = address.ReceiverName;
        existing.CountryId = address.CountryId;
        existing.CityId = address.CityId;
        existing.AddressLine = address.AddressLine;
        existing.PostalCode = address.PostalCode;
        existing.Phone = address.Phone;

        if (address.IsDefault && !existing.IsDefault)
            await ClearDefaultAsync(address.WebsiteClientID, ct);
        existing.IsDefault = address.IsDefault;

        await _context.SaveChangesAsync(ct);
        return AddressSaveResult.Success;
    }

    public async Task<bool> DeleteAsync(int addressId, int clientId, CancellationToken ct = default)
    {
        var address = await _context.WebsiteClientAddresses
            .FirstOrDefaultAsync(a => a.WebsiteClientAddressID == addressId && a.WebsiteClientID == clientId, ct);
        if (address is null) return false;

        _context.WebsiteClientAddresses.Remove(address);
        await _context.SaveChangesAsync(ct);

        // Promote another address to default when the deleted one was it.
        if (address.IsDefault)
        {
            var next = await _context.WebsiteClientAddresses
                .Where(a => a.WebsiteClientID == clientId)
                .OrderBy(a => a.WebsiteClientAddressID)
                .FirstOrDefaultAsync(ct);
            if (next is not null)
            {
                next.IsDefault = true;
                await _context.SaveChangesAsync(ct);
            }
        }

        return true;
    }

    public async Task<bool> SetDefaultAsync(int addressId, int clientId, CancellationToken ct = default)
    {
        var address = await _context.WebsiteClientAddresses
            .FirstOrDefaultAsync(a => a.WebsiteClientAddressID == addressId && a.WebsiteClientID == clientId, ct);
        if (address is null) return false;

        await ClearDefaultAsync(clientId, ct);
        address.IsDefault = true;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private async Task ClearDefaultAsync(int clientId, CancellationToken ct) =>
        await _context.WebsiteClientAddresses
            .Where(a => a.WebsiteClientID == clientId && a.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), ct);
}
