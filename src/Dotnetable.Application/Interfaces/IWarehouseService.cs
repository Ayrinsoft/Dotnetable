using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IWarehouseService
{
    Task<IReadOnlyList<Warehouse>> GetAllAsync(int websiteId, CancellationToken ct = default);
    Task<Warehouse> UpsertAsync(Warehouse warehouse, CancellationToken ct = default);
    Task EnsureDefaultAsync(int websiteId, CancellationToken ct = default);
    Task<IReadOnlyList<WarehouseStock>> GetStockAsync(int warehouseId, CancellationToken ct = default);
}
