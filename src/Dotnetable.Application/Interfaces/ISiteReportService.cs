using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

public interface ISiteReportService
{
    /// <summary>Null <paramref name="websiteId"/> returns every site (master website admin).</summary>
    Task<PagedResult<SiteReport>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    /// <summary>Null <paramref name="websiteId"/> skips the site check. A mismatch returns null.</summary>
    Task<SiteReport?> GetByIdAsync(int id, int? websiteId, CancellationToken ct = default);

    Task<SiteReport> CreateAsync(SiteReport report, CancellationToken ct = default);

    Task<bool> UpdateReviewAsync(int id, int? websiteId, SiteReportStatus status, string? adminNote, CancellationToken ct = default);

    Task DeleteAsync(int id, int? websiteId, CancellationToken ct = default);
}
