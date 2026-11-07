namespace Dotnetable.Infrastructure.Social;

/// <summary>
/// Rewrites the request URL only. Authorization and body stay as the client built them,
/// so the X OAuth 1.0a signature still covers api.x.com, not the worker.
/// </summary>
public sealed class SocialProxyHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        SocialProxyAddress.Apply(request);
        return base.SendAsync(request, cancellationToken);
    }
}
