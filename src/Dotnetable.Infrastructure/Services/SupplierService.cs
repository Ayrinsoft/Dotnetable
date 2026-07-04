using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
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
        var q = _context.Suppliers.AsNoTracking().Where(s => s.WebsiteID == websiteId);

        if (query.GetSearch(nameof(Supplier.Name)) is string name)
            q = q.Where(s => s.Name.Contains(name));
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
        await _context.Suppliers.FindAsync([id], ct);

    public async Task<Supplier> CreateAsync(Supplier supplier, CancellationToken ct = default)
    {
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(ct);
        return supplier;
    }

    public async Task UpdateAsync(Supplier supplier, CancellationToken ct = default)
    {
        var existing = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierID == supplier.SupplierID, ct);
        if (existing is null) return;

        existing.Name = supplier.Name;
        existing.Phone = supplier.Phone;
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
}
