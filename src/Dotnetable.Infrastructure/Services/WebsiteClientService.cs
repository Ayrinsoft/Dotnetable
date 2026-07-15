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
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<WebsiteClient> _hasher;

    public WebsiteClientService(AppDbContext context, IPasswordHasher<WebsiteClient> hasher)
    {
        _context = context;
        _hasher = hasher;
    }

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
            q = q.Where(c => c.ClientLevel == (byte)(ClientLevel)levelByte);

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
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.ClientLevel, (byte)level), ct);

    public async Task<(ClientSaveResult Result, WebsiteClient Client)> CreateAsync(WebsiteClient client, string password, CancellationToken ct = default)
    {
        if (await IsDuplicateAsync(client.WebsiteID, client.Email, client.Cellphone, excludeClientId: 0, ct))
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
        if (await IsDuplicateAsync(client.WebsiteID, client.Email, client.Cellphone, client.WebsiteClientID, ct))
            return ClientSaveResult.Duplicate;

        _context.WebsiteClients.Update(client);
        await _context.SaveChangesAsync(ct);
        return ClientSaveResult.Success;
    }

    /// <summary>True when another customer on the same website already owns the email or the cellphone.</summary>
    private Task<bool> IsDuplicateAsync(int websiteId, string? email, string? cellphone, int excludeClientId, CancellationToken ct) =>
        _context.WebsiteClients.AnyAsync(c =>
            c.WebsiteID == websiteId && c.WebsiteClientID != excludeClientId &&
            ((email != null && c.Email == email) || (cellphone != null && c.Cellphone == cellphone)), ct);

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
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
