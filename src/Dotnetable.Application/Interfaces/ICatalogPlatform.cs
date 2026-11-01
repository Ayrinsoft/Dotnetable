using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

/// <summary>Whether website 1 owns the shared product catalog.</summary>
public interface IPlatformCatalogService
{
    Task<bool> IsPlatformAsync(CancellationToken ct = default);
    Task<int> CatalogWebsiteIdAsync(int websiteId, CancellationToken ct = default);
}

public interface IGoogleProductTaxonomyService
{
    /// <summary>Download a Google taxonomy file and store every id for a later diff.</summary>
    Task<GoogleTaxonomySyncResult> SyncAsync(string? url = null, CancellationToken ct = default);

    Task<PagedResult<GoogleTaxonomyDiffItem>> GetDiffAsync(bool onlyMissing, int skip, int take, CancellationToken ct = default);

    /// <summary>Create website 1 categories for Google ids that are not imported yet. Null imports every missing id.</summary>
    Task<GoogleTaxonomyImportResult> ImportMissingAsync(IReadOnlyList<int>? googleIds = null, CancellationToken ct = default);

    Task<byte[]> ExportTranslationsAsync(string languageCode, CancellationToken ct = default);

    Task<ProductCategoryTranslationImportResult> ImportTranslationsAsync(Stream workbook, CancellationToken ct = default);
}

public interface ICatalogRequestService
{
    Task<CatalogRequest> SubmitAsync(CatalogRequest request, CancellationToken ct = default);
    Task<PagedResult<CatalogRequest>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);
    Task ReviewAsync(int id, CatalogRequestStatus status, string? adminNote, int reviewerMemberId, CancellationToken ct = default);
}

public interface IWebsiteContentCopyService
{
    /// <summary>
    /// Copies the source website's content and catalog rows onto the target. The source is not deleted or changed.
    /// </summary>
    Task<WebsiteContentCopyResult> CopyAsync(int sourceWebsiteId, int targetWebsiteId, CancellationToken ct = default);
}
