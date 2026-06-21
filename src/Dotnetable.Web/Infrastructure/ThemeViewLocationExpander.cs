using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Dotnetable.Web.Infrastructure;

public class ThemeViewLocationExpander : IViewLocationExpander
{
    public void PopulateValues(ViewLocationExpanderContext context)
    {
        var themeService = context.ActionContext.HttpContext.RequestServices.GetService<IThemeService>();
        if (themeService is not null)
            context.Values["theme"] = themeService.ActiveTheme;
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        if (!context.Values.TryGetValue("theme", out var theme) || string.IsNullOrEmpty(theme))
            return viewLocations;

        var themeLocations = new[]
        {
            $"/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
            $"/Themes/{theme}/Views/Shared/{{0}}.cshtml",
        };

        return themeLocations.Concat(viewLocations);
    }
}
