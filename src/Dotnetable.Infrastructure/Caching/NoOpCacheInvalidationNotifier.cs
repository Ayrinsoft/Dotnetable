using Dotnetable.Application.Interfaces;

namespace Dotnetable.Infrastructure.Caching;

/// <summary>Default notifier for hosts that don't need to push invalidation anywhere else (e.g. the API,
/// which is itself the target other hosts notify).</summary>
public class NoOpCacheInvalidationNotifier : ICacheInvalidationNotifier
{
    public Task NotifyAsync(string tag, CancellationToken ct = default) => Task.CompletedTask;
}
