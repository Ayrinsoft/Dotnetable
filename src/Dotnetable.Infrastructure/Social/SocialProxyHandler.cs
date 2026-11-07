namespace Dotnetable.Infrastructure.Social;

/// <summary>
/// Rewrites the request URL only. Authorization and body stay as the client built them,
/// so the X OAuth 1.0a signature still covers api.x.com, not the worker.
/// </summary>
public sealed class SocialProxyHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var scope = SocialProxyAmbient.Current;
        if (scope is not null && request.RequestUri is { IsAbsoluteUri: true } uri
            && !string.Equals(uri.Host, new Uri(scope.BaseUrl).Host, StringComparison.OrdinalIgnoreCase))
        {
            request.RequestUri = SocialProxyAddress.Wrap(scope.BaseUrl, uri);
            if (!string.IsNullOrEmpty(scope.Key))
                request.Headers.TryAddWithoutValidation(SocialProxyAddress.KeyHeader, scope.Key);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
