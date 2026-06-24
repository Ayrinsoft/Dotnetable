using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class LoginLogService : ILoginLogService
{
    private readonly AppDbContext _context;

    public LoginLogService(AppDbContext context) => _context = context;

    public async Task<PagedResult<LoginTry>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.LoginTries.AsNoTracking();

        if (websiteId is int wid)
            q = q.Where(l => l.WebsiteID == wid);

        if (query.GetSearch("Username") is string username)
            q = q.Where(l => l.Username.Contains(username));
        if (query.GetSearch("TryIP") is string ip)
            q = q.Where(l => l.TryIP.Contains(ip));
        if (query.GetSearch("IsSuccess") is string success && bool.TryParse(success, out var isSuccess))
            q = q.Where(l => l.IsSuccess == isSuccess);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(LoginTry.LoginTryID), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<LoginTry> { Items = items, TotalCount = total };
    }

    public async Task RecordAsync(string username, int websiteId, bool success, string ip, CancellationToken ct = default)
    {
        _context.LoginTries.Add(new LoginTry
        {
            Username = username.Length > 64 ? username[..64] : username,
            WebsiteID = websiteId,
            IsSuccess = success,
            TryIP = ip.Length > 15 ? ip[..15] : ip,
            LogTime = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync(ct);
    }
}
