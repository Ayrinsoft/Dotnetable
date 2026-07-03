using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Per-website URL redirects (301/302), supporting literal or regex source matching.
/// Admin management plus a runtime resolver the public site calls to redirect legacy paths.
/// </summary>
public interface IRedirectService
{
    // ── Admin management ────────────────────────────────────────────
    Task<PagedResult<WebsiteRedirect>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);
    Task<WebsiteRedirect?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<WebsiteRedirect> CreateAsync(WebsiteRedirect redirect, CancellationToken ct = default);
    Task UpdateAsync(WebsiteRedirect redirect, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task SetActiveAsync(int id, bool active, CancellationToken ct = default);

    // ── Runtime resolution (public site) ────────────────────────────

    /// <summary>Resolves a request path to a redirect target for a website, or null when none matches.
    /// Literal matches are tried first, then active regex rules. Increments the matched rule's hit count.</summary>
    Task<RedirectResultDto?> ResolveAsync(int websiteId, string sourcePath, CancellationToken ct = default);
}
