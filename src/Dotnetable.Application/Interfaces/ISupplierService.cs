using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Simple CRUD over <see cref="Supplier"/> — flat list, no translations.</summary>
public interface ISupplierService
{
    Task<List<Supplier>> GetAllAsync(int websiteId, CancellationToken ct = default);

    Task<PagedResult<Supplier>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default);

    Task<Supplier?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<Supplier> CreateAsync(Supplier supplier, CancellationToken ct = default);

    Task UpdateAsync(Supplier supplier, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
