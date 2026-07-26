using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Grants permanent digital entitlements on paid orders and serves the customer library with access logging.
/// Downloadable goods are external links only — no file binaries are stored or snapshotted.
/// </summary>
public interface IDigitalDeliveryService
{
    /// <summary>
    /// Creates <c>OrderDigitalAsset</c> rows for every digital order line that is not already granted.
    /// Idempotent — safe to call on every Paid transition.
    /// </summary>
    Task GrantForOrderAsync(int orderId, CancellationToken ct = default);

    Task<PagedResult<DigitalLibraryItemDto>> GetClientLibraryAsync(
        int websiteId, int clientId, GridQuery query, CancellationToken ct = default);

    Task<IReadOnlyList<DigitalLibraryItemDto>> GetByOrderAsync(
        int websiteId, int clientId, int orderId, CancellationToken ct = default);

    Task<DigitalLibraryDetailDto?> GetDetailAsync(
        int websiteId, int clientId, int orderDigitalAssetId, CancellationToken ct = default);

    /// <summary>Logs an access event (view / code / service / download-link) for the owning client.</summary>
    Task<(bool Success, string? Error)> LogAccessAsync(
        int websiteId, int clientId, int orderDigitalAssetId, DigitalAccessType accessType,
        string? ipAddress, string? userAgent, CancellationToken ct = default);

    /// <summary>
    /// Logs a download access and returns the external download URL for redirect.
    /// </summary>
    Task<DigitalDownloadResult> DownloadAsync(
        int websiteId, int clientId, int orderDigitalAssetId,
        string? ipAddress, string? userAgent, CancellationToken ct = default);
}
