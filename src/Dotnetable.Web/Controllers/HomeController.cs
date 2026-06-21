using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApiClient _api;
    private readonly WebLocalizationService _localization;

    public HomeController(ApiClient api, WebLocalizationService localization)
    {
        _api = api;
        _localization = localization;
    }

    public IActionResult Index() => View();
}
