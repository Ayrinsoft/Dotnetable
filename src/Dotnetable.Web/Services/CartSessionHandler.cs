namespace Dotnetable.Web.Services;

/// <summary>
/// Attaches the visitor's guest cart session key (see <see cref="CartSession"/>) to every API call as
/// the <c>X-Cart-Session</c> header, minting one on first use. Harmless on requests the API doesn't
/// care about the header for.
/// </summary>
public class CartSessionHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;

    public CartSessionHandler(IHttpContextAccessor accessor) => _accessor = accessor;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = _accessor.HttpContext;
        if (context is not null)
            request.Headers.TryAddWithoutValidation("X-Cart-Session", CartSession.GetOrCreate(context));

        return base.SendAsync(request, cancellationToken);
    }
}
