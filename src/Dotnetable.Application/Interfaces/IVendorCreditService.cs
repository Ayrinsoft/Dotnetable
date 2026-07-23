using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Virtual credit between a host website and a vendor (typically a linked source website).
/// Source site grants credit → host may display/sell that source's own products until credit is used up.
/// </summary>
public interface IVendorCreditService
{
    Task<VendorCreditBalanceDto> GetBalanceAsync(int vendorId, CancellationToken ct = default);

    /// <summary>Grant (positive) or adjust credit. For site vendors this is usually done by/on behalf of the source site.</summary>
    Task<(bool Success, string? Error, VendorCreditTransaction? Tx)> GrantAsync(
        int vendorId, decimal amountUsd, string? note, int? memberId, CancellationToken ct = default);

    Task<PagedResult<VendorCreditTransaction>> GetHistoryAsync(int vendorId, GridQuery query, CancellationToken ct = default);

    /// <summary>
    /// After a host checkout that includes vendor lines: debit credit, write dual settlements,
    /// and create a mirror order on each source website. Caller must already have saved host order items.
    /// </summary>
    Task SettleHostOrderAsync(int hostOrderId, CancellationToken ct = default);

    /// <summary>Restore credit and reverse settlements when a host order is cancelled/refunded.</summary>
    Task ReverseHostOrderAsync(int hostOrderId, CancellationToken ct = default);
}
