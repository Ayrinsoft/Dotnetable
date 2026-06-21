using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IApiKeyService
{
    Task<ApiKey?> ValidateAsync(string key, string? clientIp, CancellationToken ct = default);
    Task<ApiKey> CreateAsync(int websiteId, string? description, IEnumerable<string>? allowedIps, CancellationToken ct = default);
    Task RevokeAsync(int id, CancellationToken ct = default);
}
