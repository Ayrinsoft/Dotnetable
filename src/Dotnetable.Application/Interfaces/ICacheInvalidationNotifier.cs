namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Pushes a cache-tag invalidation to other processes after a write happens locally (e.g. Admin
/// notifying the API so its own in-memory cache drops the same tag immediately). The default
/// implementation is a no-op; hosts that need to reach another process override the registration.
/// </summary>
public interface ICacheInvalidationNotifier
{
    Task NotifyAsync(string tag, CancellationToken ct = default);
}
