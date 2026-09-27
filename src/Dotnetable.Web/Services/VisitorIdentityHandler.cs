namespace Dotnetable.Web.Services;

/// <summary>
/// Forwards the visitor (not this server) on API calls: <c>X-Visitor-Id</c>, <c>X-Client-Ip</c> and
/// <c>X-Visitor-User-Agent</c>. Crawlers are skipped so they are neither cookied nor counted.
/// </summary>
public class VisitorIdentityHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;

    public VisitorIdentityHandler(IHttpContextAccessor accessor) => _accessor = accessor;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = _accessor.HttpContext;
        if (context is not null && !BotDetector.IsBot(context))
        {
            request.Headers.TryAddWithoutValidation("X-Visitor-Id", VisitorIdentity.GetOrCreate(context));

            var ip = context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrWhiteSpace(ip))
                request.Headers.TryAddWithoutValidation("X-Client-Ip", ip);

            var agent = context.Request.Headers.UserAgent.ToString();
            if (!string.IsNullOrWhiteSpace(agent))
                request.Headers.TryAddWithoutValidation("X-Visitor-User-Agent", agent);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
