using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>Switches the visitor's UI language (the <c>lang</c> cookie every content controller
/// reads) to one of the website's own active languages, then returns to where they were.</summary>
public class LanguageController : Controller
{
    [HttpGet]
    public IActionResult Set(string code, string? returnUrl = null)
    {
        Response.Cookies.Append("lang", code, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
        });

        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }
}
