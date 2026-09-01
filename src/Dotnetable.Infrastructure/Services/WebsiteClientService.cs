using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class WebsiteClientService : IWebsiteClientService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IPasswordHasher<WebsiteClient> _hasher;

    public WebsiteClientService(IDbContextFactory<AppDbContext> contextFactory, IPasswordHasher<WebsiteClient> hasher)
    {
        _contextFactory = contextFactory;
        _hasher = hasher;
    }

    public async Task<WebsiteClient?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.WebsiteClients.FindAsync([id], ct);
    }

    public async Task<PagedResult<WebsiteClient>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

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
            q = q.Where(c => c.ClientLevel == (byte)(ClientLevel)levelByte);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(WebsiteClient.WebsiteClientID))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<WebsiteClient> { Items = items, TotalCount = total };
    }

    public async Task<IReadOnlyList<WebsiteClient>> SearchAsync(int? websiteId, string? term, int take = 20, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.WebsiteClients.AsNoTracking().Where(c => c.Active);
        if (websiteId is int wid)
            q = q.Where(c => c.WebsiteID == wid);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var t = term.Trim();
            q = q.Where(c =>
                (c.Email != null && c.Email.Contains(t))
                || (c.Cellphone != null && c.Cellphone.Contains(t))
                || (c.Givenname != null && c.Givenname.Contains(t))
                || (c.Surname != null && c.Surname.Contains(t))
                || ((c.Givenname ?? "") + " " + (c.Surname ?? "")).Contains(t));
        }

        var limit = take < 1 ? 20 : Math.Min(take, 50);
        return await q
            .OrderBy(c => c.Surname)
            .ThenBy(c => c.Givenname)
            .ThenBy(c => c.WebsiteClientID)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task SetActiveAsync(int id, bool active, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        await _context.WebsiteClients.Where(c => c.WebsiteClientID == id)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Active, active), ct);
    }

    public async Task SetLevelAsync(int id, ClientLevel level, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        await _context.WebsiteClients.Where(c => c.WebsiteClientID == id)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.ClientLevel, (byte)level), ct);
    }

    public async Task<(ClientSaveResult Result, WebsiteClient Client)> CreateAsync(WebsiteClient client, string password, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (await IsDuplicateAsync(_context, client.WebsiteID, client.Email, client.Cellphone, excludeClientId: 0, ct))
            return (ClientSaveResult.Duplicate, client);

        client.HashKey = Guid.NewGuid();
        client.RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow);
        client.Password = _hasher.HashPassword(client, password);
        _context.WebsiteClients.Add(client);
        await _context.SaveChangesAsync(ct);
        return (ClientSaveResult.Success, client);
    }

    public async Task<ClientSaveResult> UpdateAsync(WebsiteClient client, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (await IsDuplicateAsync(_context, client.WebsiteID, client.Email, client.Cellphone, client.WebsiteClientID, ct))
            return ClientSaveResult.Duplicate;

        _context.WebsiteClients.Update(client);
        await _context.SaveChangesAsync(ct);
        return ClientSaveResult.Success;
    }

    /// <summary>True when another customer on the same website already owns the email or the cellphone.</summary>
    // Takes the caller's context so the duplicate check and the insert that follows see the same
    // snapshot of the table.
    private static Task<bool> IsDuplicateAsync(AppDbContext _context, int websiteId, string? email, string? cellphone, int excludeClientId, CancellationToken ct) =>
        _context.WebsiteClients.AnyAsync(c =>
            c.WebsiteID == websiteId && c.WebsiteClientID != excludeClientId &&
            ((email != null && c.Email == email) || (cellphone != null && c.Cellphone == cellphone)), ct);

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var client = await _context.WebsiteClients.FindAsync([id], ct);
        if (client is null) return;
        // Remove dependent rows first (FK to WebsiteClient): activation/reset codes and saved addresses.
        var codes = _context.WebsiteClientForgetPasswords.Where(f => f.WebsiteClientID == id);
        _context.WebsiteClientForgetPasswords.RemoveRange(codes);
        var addresses = _context.WebsiteClientAddresses.Where(a => a.WebsiteClientID == id);
        _context.WebsiteClientAddresses.RemoveRange(addresses);
        _context.WebsiteClients.Remove(client);
        await _context.SaveChangesAsync(ct);
    }
}
