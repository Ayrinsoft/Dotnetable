using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Suppliers (external stock vendors and inter-site tax counterparties) with full tax identity fields.
/// </summary>
public interface ISupplierService
{
    Task<List<Supplier>> GetAllAsync(int websiteId, CancellationToken ct = default);

    Task<PagedResult<Supplier>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default);

    Task<Supplier?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<Supplier> CreateAsync(Supplier supplier, CancellationToken ct = default);

    Task UpdateAsync(Supplier supplier, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Creates or returns the host-site supplier row that represents a linked source website
    /// for settlement and dual-party tax reporting.
    /// </summary>
    Task<int> EnsureLinkedWebsiteSupplierAsync(
        int hostWebsiteId, int sourceWebsiteId, string? displayName, CancellationToken ct = default);
}
