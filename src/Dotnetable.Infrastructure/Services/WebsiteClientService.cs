using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class WebsiteClientService : IWebsiteClientService
{
    private readonly AppDbContext _context;

    public WebsiteClientService(AppDbContext context) => _context = context;

    public async Task<WebsiteClient?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.WebsiteClients.FindAsync([id], ct);

    public async Task<PagedResult<WebsiteClient>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.WebsiteClients.AsNoTracking();

        if (websiteId is int wid)
            q = q.Where(c => c.WebsiteID == wid);

        if (query.GetSearch("Email") is string email)
            q = q.Where(c => c.Email != null && c.Email.Contains(email));
        if (query.GetSearch("Cellphone") is string cell)
            q = q.Where(c => c.Cellphone != null && c.Cellphone.Contains(cell));
        if (query.GetSearch("Fullname") is string fullname)
            q = q.Where(c => ((c.Givenname ?? "") + " " + (c.Surname ?? "")).Contains(fullname));
        if (query.GetSearch("Active") is string active && bool.TryParse(active, out var isActive))
            q = q.Where(c => c.Active == isActive);
        if (query.GetSearch("ClientLevel") is string levelText && byte.TryParse(levelText, out var levelByte))
            q = q.Where(c => c.ClientLevel == (ClientLevel)levelByte);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(WebsiteClient.WebsiteClientID))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<WebsiteClient> { Items = items, TotalCount = total };
    }

    public async Task SetActiveAsync(int id, bool active, CancellationToken ct = default) =>
        await _context.WebsiteClients.Where(c => c.WebsiteClientID == id)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Active, active), ct);

    public async Task SetLevelAsync(int id, ClientLevel level, CancellationToken ct = default) =>
        await _context.WebsiteClients.Where(c => c.WebsiteClientID == id)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.ClientLevel, level), ct);

    public async Task UpdateAsync(WebsiteClient client, CancellationToken ct = default)
    {
        _context.WebsiteClients.Update(client);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var client = await _context.WebsiteClients.FindAsync([id], ct);
        if (client is null) return;
        // Remove any outstanding activation / reset codes first (FK to WebsiteClient).
        var codes = _context.WebsiteClientForgetPasswords.Where(f => f.WebsiteClientID == id);
        _context.WebsiteClientForgetPasswords.RemoveRange(codes);
        _context.WebsiteClients.Remove(client);
        await _context.SaveChangesAsync(ct);
    }
}
