using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected int CurrentWebsiteId =>
        HttpContext.Items.TryGetValue("WebsiteId", out var id) && id is int websiteId
            ? websiteId
            : throw new InvalidOperationException("WebsiteId not found in context.");

    protected ApiKey CurrentApiKey =>
        HttpContext.Items.TryGetValue("ApiKey", out var key) && key is ApiKey apiKey
            ? apiKey
            : throw new InvalidOperationException("ApiKey not found in context.");
}
