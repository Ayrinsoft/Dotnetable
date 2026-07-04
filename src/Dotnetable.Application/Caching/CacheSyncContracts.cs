namespace Dotnetable.Application.Caching;

/// <summary>Shared contract between Admin (caller) and the API's <c>POST api/cache/invalidate</c>
/// endpoint (callee) — keeps both sides in sync without either executable referencing the other.</summary>
public static class CacheSyncContracts
{
    /// <summary>Header carrying the shared secret configured as <c>Internal:SyncSecret</c> on both hosts.</summary>
    public const string InternalKeyHeader = "X-Internal-Key";
}

public record CacheInvalidateRequest(string Tag);
