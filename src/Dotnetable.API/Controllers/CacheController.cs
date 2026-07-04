using Dotnetable.Application.Caching;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Internal endpoint the Admin process calls right after a write (Menu/Category/Page/Post) so the
/// API's own in-memory cache drops the same tag immediately, instead of waiting out its TTL.
/// Not for public/client use — guarded by a shared secret, not the customer JWT/website-key scheme.
/// </summary>
[ApiController]
[Route("api/cache")]
public class CacheController : ControllerBase
{
    private readonly ICacheService _cache;
    private readonly IConfiguration _configuration;

    public CacheController(ICacheService cache, IConfiguration configuration)
    {
        _cache = cache;
        _configuration = configuration;
    }

    [HttpPost("invalidate")]
    public IActionResult Invalidate([FromBody] CacheInvalidateRequest request)
    {
        var expected = _configuration["Internal:SyncSecret"];
        if (string.IsNullOrEmpty(expected) ||
            !Request.Headers.TryGetValue(CacheSyncContracts.InternalKeyHeader, out var provided) ||
            provided != expected)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Tag))
            return BadRequest(new { message = "Tag is required." });

        _cache.RemoveByTag(request.Tag);
        return NoContent();
    }
}
