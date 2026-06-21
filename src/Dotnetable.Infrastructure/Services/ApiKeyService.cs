using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly AppDbContext _context;

    public ApiKeyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiKey?> ValidateAsync(string key, string? clientIp, CancellationToken ct = default)
    {
        var apiKey = await _context.ApiKeys
            .FirstOrDefaultAsync(a => a.Key == key && a.IsActive, ct);

        if (apiKey is null) return null;
        if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt < DateTime.UtcNow) return null;

        var allowedIps = apiKey.GetAllowedIpList().ToList();
        if (allowedIps.Count > 0 && !string.IsNullOrWhiteSpace(clientIp) && !allowedIps.Contains(clientIp))
            return null;

        apiKey.LastUsedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return apiKey;
    }

    public async Task<ApiKey> CreateAsync(int websiteId, string? description, IEnumerable<string>? allowedIps, CancellationToken ct = default)
    {
        var apiKey = new ApiKey
        {
            WebsiteId = websiteId,
            Key = GenerateKey(),
            Description = description,
            AllowedIps = allowedIps is not null ? string.Join(",", allowedIps) : null
        };

        _context.ApiKeys.Add(apiKey);
        await _context.SaveChangesAsync(ct);

        return apiKey;
    }

    public async Task RevokeAsync(int id, CancellationToken ct = default)
    {
        var apiKey = await _context.ApiKeys.FindAsync([id], ct);
        if (apiKey is null) return;

        apiKey.IsActive = false;
        await _context.SaveChangesAsync(ct);
    }

    private static string GenerateKey() =>
        Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "-").Replace("/", "_").Replace("=", "");
}
