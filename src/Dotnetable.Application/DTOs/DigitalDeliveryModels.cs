namespace Dotnetable.Application.DTOs;

/// <summary>One digital entitlement in the customer library (always available after paid purchase).</summary>
public class DigitalLibraryItemDto
{
    public int OrderDigitalAssetID { get; init; }
    public int OrderID { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public int OrderItemID { get; init; }
    public int ProductID { get; init; }
    public byte ProductType { get; init; }
    public string Title { get; init; } = string.Empty;
    /// <summary>True when a download URL was granted (external link only).</summary>
    public bool HasDownloadUrl { get; init; }
    public bool HasServiceUrl { get; init; }
    public bool HasDeliveryNote { get; init; }
    public DateTime GrantedAt { get; init; }
    public int AccessCount { get; init; }
    public DateTime? LastAccessedAt { get; init; }
}

/// <summary>Full digital entitlement detail for the customer panel.</summary>
public sealed class DigitalLibraryDetailDto : DigitalLibraryItemDto
{
    /// <summary>License / instructions text.</summary>
    public string? DigitalDeliveryNote { get; init; }

    /// <summary>Service URL.</summary>
    public string? DigitalServiceUrl { get; init; }

    /// <summary>External download link (never a hosted file on this platform).</summary>
    public string? DigitalDownloadUrl { get; init; }

    public IReadOnlyList<DigitalAccessLogDto> RecentAccess { get; init; } = Array.Empty<DigitalAccessLogDto>();
}

public sealed class DigitalAccessLogDto
{
    public long DigitalAccessLogID { get; init; }
    public byte AccessType { get; init; }
    public string AccessTypeName { get; init; } = string.Empty;
    public DateTime AccessedAt { get; init; }
    public string? IpAddress { get; init; }
}

/// <summary>Result of a logged download request — always a redirect URL, never a file stream.</summary>
public sealed class DigitalDownloadResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    /// <summary>External URL to open / download.</summary>
    public string? DownloadUrl { get; init; }

    public static DigitalDownloadResult Fail(string error) => new() { Success = false, Error = error };
    public static DigitalDownloadResult Ok(string url) => new() { Success = true, DownloadUrl = url };
}
