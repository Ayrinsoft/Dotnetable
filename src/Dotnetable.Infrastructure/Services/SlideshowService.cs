using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class SlideshowService : ISlideshowService
{
    private readonly AppDbContext _context;

    public SlideshowService(AppDbContext context) => _context = context;

    // ── Slideshows ──────────────────────────────────────────────────

    public async Task<List<Slideshow>> GetSlideshowsAsync(int? websiteId, CancellationToken ct = default)
    {
        var q = _context.Slideshows.AsNoTracking().Include(s => s.SlideshowSlides).AsQueryable();
        if (websiteId is int wid)
            q = q.Where(s => s.WebsiteID == wid);
        return await q.OrderBy(s => s.Name).ToListAsync(ct);
    }

    public async Task<PagedResult<Slideshow>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Slideshows.AsNoTracking().Include(s => s.SlideshowSlides).AsQueryable();
        if (websiteId is int wid)
            q = q.Where(s => s.WebsiteID == wid);

        if (query.GetSearch(nameof(Slideshow.Name)) is string name)
            q = q.Where(s => s.Name.Contains(name));
        if (query.GetSearch(nameof(Slideshow.PlacementKey)) is string placement)
            q = q.Where(s => s.PlacementKey != null && s.PlacementKey.Contains(placement));
        if (query.GetSearch(nameof(Slideshow.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(s => s.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Slideshow.Name))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Slideshow> { Items = items, TotalCount = total };
    }

    public async Task<Slideshow?> GetSlideshowAsync(int slideshowId, CancellationToken ct = default) =>
        await _context.Slideshows.FindAsync([slideshowId], ct);

    public async Task<Slideshow> CreateSlideshowAsync(Slideshow slideshow, CancellationToken ct = default)
    {
        slideshow.CreatedAt = DateTime.UtcNow;
        _context.Slideshows.Add(slideshow);
        await _context.SaveChangesAsync(ct);
        return slideshow;
    }

    public async Task UpdateSlideshowAsync(Slideshow slideshow, CancellationToken ct = default)
    {
        _context.Slideshows.Update(slideshow);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteSlideshowAsync(int slideshowId, CancellationToken ct = default)
    {
        var slideshow = await _context.Slideshows
            .Include(s => s.SlideshowSlides)
            .FirstOrDefaultAsync(s => s.SlideshowID == slideshowId, ct);
        if (slideshow is null) return;

        _context.SlideshowSlides.RemoveRange(slideshow.SlideshowSlides);
        _context.Slideshows.Remove(slideshow);
        await _context.SaveChangesAsync(ct);
    }

    // ── Slides ──────────────────────────────────────────────────────

    public async Task<List<SlideshowSlide>> GetSlidesAsync(int slideshowId, CancellationToken ct = default) =>
        await _context.SlideshowSlides.AsNoTracking()
            .Include(s => s.File)
            .Include(s => s.MobileFile)
            .Where(s => s.SlideshowID == slideshowId)
            .OrderBy(s => s.SortOrder).ThenBy(s => s.SlideshowSlideID)
            .ToListAsync(ct);

    public async Task<SlideshowSlide?> GetSlideAsync(int slideId, CancellationToken ct = default) =>
        await _context.SlideshowSlides.FindAsync([slideId], ct);

    public async Task<SlideshowSlide> CreateSlideAsync(SlideshowSlide slide, CancellationToken ct = default)
    {
        _context.SlideshowSlides.Add(slide);
        await _context.SaveChangesAsync(ct);
        return slide;
    }

    public async Task UpdateSlideAsync(SlideshowSlide slide, CancellationToken ct = default)
    {
        _context.SlideshowSlides.Update(slide);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteSlideAsync(int slideId, CancellationToken ct = default)
    {
        var slide = await _context.SlideshowSlides.FindAsync([slideId], ct);
        if (slide is null) return;

        _context.SlideshowSlides.Remove(slide);
        await _context.SaveChangesAsync(ct);
    }

    // ── Public read ─────────────────────────────────────────────────

    public async Task<SlideshowDto?> GetByPlacementAsync(int websiteId, string placementKey, CancellationToken ct = default)
    {
        var slideshow = await LoadActiveSlideshowsQuery(websiteId)
            .FirstOrDefaultAsync(s => s.PlacementKey == placementKey, ct);
        return slideshow is null ? null : Project(slideshow);
    }

    public async Task<SlideshowDto?> GetByIdAsync(int websiteId, int slideshowId, CancellationToken ct = default)
    {
        var slideshow = await LoadActiveSlideshowsQuery(websiteId)
            .FirstOrDefaultAsync(s => s.SlideshowID == slideshowId, ct);
        return slideshow is null ? null : Project(slideshow);
    }

    private IQueryable<Slideshow> LoadActiveSlideshowsQuery(int websiteId)
    {
        var now = DateTime.UtcNow;
        return _context.Slideshows.AsNoTracking()
            .Where(s => s.WebsiteID == websiteId && s.IsActive)
            .Include(s => s.SlideshowSlides.Where(i => i.IsActive
                    && (i.StartAt == null || i.StartAt <= now)
                    && (i.EndAt == null || i.EndAt >= now)))
                .ThenInclude(i => i.File)
            .Include(s => s.SlideshowSlides.Where(i => i.IsActive
                    && (i.StartAt == null || i.StartAt <= now)
                    && (i.EndAt == null || i.EndAt >= now)))
                .ThenInclude(i => i.MobileFile);
    }

    // ── Projection helpers ──────────────────────────────────────────

    private static SlideshowDto Project(Slideshow slideshow) => new()
    {
        SlideshowID = slideshow.SlideshowID,
        Name = slideshow.Name,
        PlacementKey = slideshow.PlacementKey,
        TransitionEffect = (SlideTransitionEffect)slideshow.TransitionEffect,
        AutoPlay = slideshow.AutoPlay,
        IntervalMs = slideshow.IntervalMs,
        ShowArrows = slideshow.ShowArrows,
        ShowDots = slideshow.ShowDots,
        EnableLightbox = slideshow.EnableLightbox,
        AspectRatio = slideshow.AspectRatio,
        Slides = slideshow.SlideshowSlides
            .Where(i => i.IsActive)
            .OrderBy(i => i.SortOrder).ThenBy(i => i.SlideshowSlideID)
            .Select(ProjectSlide)
            .ToList(),
    };

    private static SlideDto ProjectSlide(SlideshowSlide slide) => new()
    {
        SlideshowSlideID = slide.SlideshowSlideID,
        ImageUrl = slide.File.CNDUrl ?? string.Empty,
        MobileImageUrl = slide.MobileFile?.CNDUrl,
        AltText = slide.File.AltText ?? slide.Title,
        Title = slide.Title,
        Caption = slide.Caption,
        ButtonText = slide.ButtonText,
        LinkUrl = slide.LinkUrl,
        OpenInNewTab = slide.OpenInNewTab,
    };
}
