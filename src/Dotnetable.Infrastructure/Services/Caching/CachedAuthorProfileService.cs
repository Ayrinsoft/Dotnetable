using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Caching;

namespace Dotnetable.Infrastructure.Services.Caching;

/// <summary>
/// Caches the public author page of <see cref="AuthorProfileService"/>. Every write drops the
/// <c>author</c> tag and also the <c>post</c> tag: post listings carry the author box (name, bio,
/// photo), so a bio edit must not wait for the post cache to expire.
/// </summary>
public class CachedAuthorProfileService : IAuthorProfileService
{
    private const string Tag = "author";
    private const string PostTag = "post";

    private readonly AuthorProfileService _inner;
    private readonly ICacheService _cache;
    private readonly ICacheInvalidationNotifier _notifier;
    private readonly TimeSpan _ttl;

    public CachedAuthorProfileService(AuthorProfileService inner, ICacheService cache, ICacheInvalidationNotifier notifier, CacheOptions options)
    {
        _inner = inner;
        _cache = cache;
        _notifier = notifier;
        _ttl = options.DefaultTtl;
    }

    public Task<List<AuthorListRow>> GetAuthorsAsync(int? websiteId, CancellationToken ct = default) =>
        _inner.GetAuthorsAsync(websiteId, ct);

    public Task<AuthorProfile?> GetByMemberAsync(int memberId, CancellationToken ct = default) =>
        _inner.GetByMemberAsync(memberId, ct);

    public async Task<AuthorProfile> SaveAsync(AuthorProfile profile, IReadOnlyList<AuthorProfileTranslation> translations, CancellationToken ct = default)
    {
        var result = await _inner.SaveAsync(profile, translations, ct);
        await InvalidateAsync(ct);
        return result;
    }

    public async Task<AuthorResumeItem> SaveResumeItemAsync(int memberId, AuthorResumeItem item, IReadOnlyList<AuthorResumeItemTranslation> translations, CancellationToken ct = default)
    {
        var result = await _inner.SaveResumeItemAsync(memberId, item, translations, ct);
        await InvalidateAsync(ct);
        return result;
    }

    public async Task DeleteResumeItemAsync(int memberId, int itemId, CancellationToken ct = default)
    {
        await _inner.DeleteResumeItemAsync(memberId, itemId, ct);
        await InvalidateAsync(ct);
    }

    public Task<AuthorPageDto?> GetPublicAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default) =>
        _cache.GetOrCreateAsync(
            $"author:page:{websiteId}:{slug.ToLowerInvariant()}:{languageCode}",
            [Tag],
            _ttl,
            () => _inner.GetPublicAsync(websiteId, slug, languageCode, ct));

    private async Task InvalidateAsync(CancellationToken ct)
    {
        _cache.RemoveByTag(Tag);
        _cache.RemoveByTag(PostTag);
        await _notifier.NotifyAsync(Tag, ct);
        await _notifier.NotifyAsync(PostTag, ct);
    }
}
