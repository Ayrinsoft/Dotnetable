using System.Net.Http.Headers;

namespace Dotnetable.Web.Services;

/// <summary>
/// Forwards the customer's JWT (stored in the <see cref="ClientAuth.TokenCookie"/> cookie) to the API
/// as a Bearer token, so authenticated calls made on behalf of the signed-in visitor are authorized.
/// </summary>
public class BearerTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;

    public BearerTokenHandler(IHttpContextAccessor accessor) => _accessor = accessor;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _accessor.HttpContext?.Request.Cookies[ClientAuth.TokenCookie];
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
