using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>Website scope for the request, taken from the <c>X-Website-Id</c> header.</summary>
    protected int CurrentWebsiteId =>
        Request.Headers.TryGetValue("X-Website-Id", out var value) && int.TryParse(value, out var websiteId)
            ? websiteId
            : throw new InvalidOperationException("X-Website-Id header is missing or invalid.");
}
