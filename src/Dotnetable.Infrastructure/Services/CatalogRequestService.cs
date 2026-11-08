using Dotnetable.Application;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Catalog;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class CatalogRequestService : ICatalogRequestService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IAdminNotificationService _notifications;

    public CatalogRequestService(
        IDbContextFactory<AppDbContext> contextFactory,
        IAdminNotificationService notifications)
    {
        _contextFactory = contextFactory;
        _notifications = notifications;
    }

    public async Task<CatalogRequest> SubmitAsync(CatalogRequest request, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        if (!await PlatformCatalog.IsPlatformAsync(context, ct))
            throw new InvalidOperationException("Catalog requests are only used when website 1 is a platform.");
        if (request.WebsiteID == AppConstants.MasterWebsiteId)
            throw new InvalidOperationException("Website 1 adds products and categories directly.");
        if (request.RequestedByMemberID <= 0)
            throw new InvalidOperationException("A signed-in member is required.");

        request.ProductTitle = request.ProductTitle.Trim();
        if (string.IsNullOrWhiteSpace(request.ProductTitle))
            throw new InvalidOperationException("Product title is required.");

        request.Kind = request.Kind == (byte)CatalogRequestKind.CategoryAndProduct
            ? (byte)CatalogRequestKind.CategoryAndProduct
            : (byte)CatalogRequestKind.Product;
        if (request.Kind == (byte)CatalogRequestKind.CategoryAndProduct
            && string.IsNullOrWhiteSpace(request.ProposedCategoryName))
            throw new InvalidOperationException("Category name is required when asking for a new category.");

        if (request.ParentCategoryID is int parentId)
        {
            var parentOk = await context.ProductCategories.AnyAsync(c =>
                c.ProductCategoryID == parentId && c.WebsiteID == AppConstants.MasterWebsiteId && c.IsActive, ct);
            if (!parentOk) request.ParentCategoryID = null;
        }

        request.ProposedCategoryName = string.IsNullOrWhiteSpace(request.ProposedCategoryName)
            ? null
            : request.ProposedCategoryName.Trim();
        request.Details = string.IsNullOrWhiteSpace(request.Details) ? null : request.Details.Trim();
        request.Status = (byte)CatalogRequestStatus.Pending;
        request.CreatedAt = DateTime.UtcNow;
        request.AdminNote = null;
        request.ReviewedAt = null;
        request.ReviewedByMemberID = null;

        context.CatalogRequests.Add(request);
        await context.SaveChangesAsync(ct);

        var siteName = await context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == request.WebsiteID)
            .Select(w => w.TradeName)
            .FirstOrDefaultAsync(ct) ?? $"Website {request.WebsiteID}";
        var kind = request.Kind == (byte)CatalogRequestKind.CategoryAndProduct ? "category and product" : "product";
        var title = $"Catalog request: {kind}";
        var message = $"{siteName} asked to add \"{request.ProductTitle}\".";
        if (!string.IsNullOrWhiteSpace(request.ProposedCategoryName))
            message += $" Category: {request.ProposedCategoryName}.";

        await _notifications.NotifySiteAdminsAsync(
            AppConstants.MasterWebsiteId,
            AdminNotificationType.CatalogRequest,
            title,
            message,
            "/catalog/requests",
            request.CatalogRequestID,
            ct);
        return request;
    }

    public async Task<PagedResult<CatalogRequest>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var q = context.CatalogRequests.AsNoTracking()
            .Include(r => r.Website)
            .Include(r => r.RequestedByMember)
            .AsQueryable();
        if (websiteId is int wid)
            q = q.Where(r => r.WebsiteID == wid);
        if (query.GetSearch(nameof(CatalogRequest.ProductTitle)) is string title)
            q = q.Where(r => r.ProductTitle.Contains(title));
        if (query.GetSearch(nameof(CatalogRequest.Status)) is string statusText && byte.TryParse(statusText, out var status))
            q = q.Where(r => r.Status == status);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(CatalogRequest.CreatedAt), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);
        return new PagedResult<CatalogRequest> { Items = items, TotalCount = total };
    }

    public async Task ReviewAsync(int id, CatalogRequestStatus status, string? adminNote, int reviewerMemberId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var request = await context.CatalogRequests.FirstOrDefaultAsync(r => r.CatalogRequestID == id, ct);
        if (request is null) return;
        request.Status = (byte)status;
        request.AdminNote = string.IsNullOrWhiteSpace(adminNote) ? null : adminNote.Trim();
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedByMemberID = reviewerMemberId > 0 ? reviewerMemberId : null;
        await context.SaveChangesAsync(ct);
    }
}
