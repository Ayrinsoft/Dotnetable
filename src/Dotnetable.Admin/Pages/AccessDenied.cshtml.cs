using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetable.Admin.Pages;

[AllowAnonymous]
public class AccessDeniedModel : PageModel
{
}
