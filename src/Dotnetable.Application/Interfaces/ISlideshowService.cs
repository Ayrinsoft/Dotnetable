using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Reusable image slideshows and their ordered slides. A slideshow can be rendered on the public
/// website in two ways: assigned to a named <see cref="Slideshow.PlacementKey"/> zone (fetched by
/// key, like a widget area), or embedded anywhere inside a Post/Page body via its
/// <c>[slideshow:ID]</c> shortcode (fetched by id). All slideshows are scoped to a single
/// <see cref="Website"/>. The admin panel uses the management methods directly; the public website
/// consumes the read projections through the API.
/// </summary>
public interface ISlideshowService
{
    // ── Slideshows (admin management) ───────────────────────────────

    /// <summary>All slideshows for a website (or every website when <paramref name="websiteId"/> is null — master only).</summary>
    Task<List<Slideshow>> GetSlideshowsAsync(int? websiteId, CancellationToken ct = default);

    /// <summary>Paged / sorted / filtered slideshows for the admin grid.</summary>
    Task<PagedResult<Slideshow>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    Task<Slideshow?> GetSlideshowAsync(int slideshowId, CancellationToken ct = default);
    Task<Slideshow> CreateSlideshowAsync(Slideshow slideshow, CancellationToken ct = default);
    Task UpdateSlideshowAsync(Slideshow slideshow, CancellationToken ct = default);
    Task DeleteSlideshowAsync(int slideshowId, CancellationToken ct = default);

    // ── Slides (admin management) ───────────────────────────────────

    /// <summary>Every slide of a slideshow, ordered by sort order (includes inactive).</summary>
    Task<List<SlideshowSlide>> GetSlidesAsync(int slideshowId, CancellationToken ct = default);

    Task<SlideshowSlide?> GetSlideAsync(int slideId, CancellationToken ct = default);
    Task<SlideshowSlide> CreateSlideAsync(SlideshowSlide slide, CancellationToken ct = default);
    Task UpdateSlideAsync(SlideshowSlide slide, CancellationToken ct = default);
    Task DeleteSlideAsync(int slideId, CancellationToken ct = default);

    // ── Public read (website rendering) ─────────────────────────────

    /// <summary>The active slideshow assigned to a placement key, projected to a render-ready DTO.
    /// Null when no active slideshow is assigned to that key.</summary>
    Task<SlideshowDto?> GetByPlacementAsync(int websiteId, string placementKey, CancellationToken ct = default);

    /// <summary>A single active slideshow by id, scoped to the website — used to resolve a
    /// <c>[slideshow:ID]</c> shortcode embedded inside Post/Page content. Null when not found,
    /// inactive, or belonging to a different website.</summary>
    Task<SlideshowDto?> GetByIdAsync(int websiteId, int slideshowId, CancellationToken ct = default);
}
