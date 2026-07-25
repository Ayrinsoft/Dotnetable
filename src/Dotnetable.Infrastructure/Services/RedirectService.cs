using System.Text.RegularExpressions;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class RedirectService : IRedirectService
{
    private readonly AppDbContext _context;

    public RedirectService(AppDbContext context) => _context = context;

    // ── Admin management ────────────────────────────────────────────

    public async Task<PagedResult<WebsiteRedirect>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.WebsiteRedirects.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(r => r.WebsiteID == wid);

        if (query.GetSearch(nameof(WebsiteRedirect.SourcePath)) is string source)
            q = q.Where(r => r.SourcePath.Contains(source));
        if (query.GetSearch(nameof(WebsiteRedirect.TargetPath)) is string target)
            q = q.Where(r => r.TargetPath.Contains(target));
        if (query.GetSearch(nameof(WebsiteRedirect.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(r => r.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(WebsiteRedirect.WebsiteRedirectID))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<WebsiteRedirect> { Items = items, TotalCount = total };
    }

    public async Task<WebsiteRedirect?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.WebsiteRedirects.FindAsync([id], ct);

    public async Task<WebsiteRedirect> CreateAsync(WebsiteRedirect redirect, CancellationToken ct = default)
    {
        redirect.CreatedAt = DateTime.UtcNow;
        if (redirect.StatusCode == 0)
            redirect.StatusCode = 301;
        _context.WebsiteRedirects.Add(redirect);
        await _context.SaveChangesAsync(ct);
        return redirect;
    }

    public async Task UpdateAsync(WebsiteRedirect redirect, CancellationToken ct = default)
    {
        _context.WebsiteRedirects.Update(redirect);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.WebsiteRedirects.FindAsync([id], ct);
        if (entity is null) return;
        _context.WebsiteRedirects.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task SetActiveAsync(int id, bool active, CancellationToken ct = default) =>
        await _context.WebsiteRedirects.Where(r => r.WebsiteRedirectID == id)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsActive, active), ct);

    // ── Runtime resolution ──────────────────────────────────────────

    public async Task<RedirectResultDto?> ResolveAsync(int websiteId, string sourcePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sourcePath)) return null;
        var normalized = Normalize(sourcePath);

        var rules = await _context.WebsiteRedirects
            .Where(r => r.WebsiteID == websiteId && r.IsActive)
            .ToListAsync(ct);

        // Literal matches first (cheap, unambiguous), then regex rules in id order.
        var match = rules.FirstOrDefault(r => !r.IsRegex &&
            string.Equals(Normalize(r.SourcePath), normalized, StringComparison.OrdinalIgnoreCase));

        string? target = match?.TargetPath;

        if (match is null)
        {
            foreach (var r in rules.Where(r => r.IsRegex).OrderBy(r => r.WebsiteRedirectID))
            {
                try
                {
                    var regex = new Regex(r.SourcePath, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
                    if (regex.IsMatch(normalized))
                    {
                        match = r;
                        target = regex.Replace(normalized, r.TargetPath);
                        break;
                    }
                }
                catch (Exception) { /* skip invalid or timed-out patterns */ }
            }
        }

        if (match is null || string.IsNullOrWhiteSpace(target)) return null;

        // Best-effort hit counter — never fail the redirect if this update throws.
        try
        {
            await _context.WebsiteRedirects.Where(r => r.WebsiteRedirectID == match.WebsiteRedirectID)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.HitCount, r => r.HitCount + 1), ct);
        }
        catch (Exception) { }

        return new RedirectResultDto { TargetPath = target, StatusCode = match.StatusCode };
    }

    private static string Normalize(string path)
    {
        var p = path.Trim();
        if (!p.StartsWith('/')) p = "/" + p;
        if (p.Length > 1) p = p.TrimEnd('/');
        return p;
    }
}
