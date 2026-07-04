using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Read-only ledger over <see cref="StockMovement"/> rows for the admin panel.</summary>
public interface IStockMovementService
{
    Task<PagedResult<StockMovement>> GetPagedAsync(int websiteId, GridQuery query, int? variantId, CancellationToken ct = default);
}
