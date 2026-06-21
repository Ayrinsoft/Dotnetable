namespace Dotnetable.Web.Services;

public interface IThemeService
{
    string ActiveTheme { get; }
    string GetViewPath(string viewName);
    string GetLayoutPath();
    IEnumerable<string> GetAvailableThemes();
    void SetTheme(string themeName);
}

public class ThemeService : IThemeService
{
    private readonly IWebHostEnvironment _env;
    private string _activeTheme;

    public ThemeService(IWebHostEnvironment env, IConfiguration configuration)
    {
        _env = env;
        _activeTheme = configuration["Theme:Active"] ?? "Default";
    }

    public string ActiveTheme => _activeTheme;

    public string GetViewPath(string viewName) =>
        $"/Themes/{_activeTheme}/Views/{viewName}.cshtml";

    public string GetLayoutPath() =>
        $"/Themes/{_activeTheme}/Views/Shared/_Layout.cshtml";

    public IEnumerable<string> GetAvailableThemes()
    {
        var themesPath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "Themes");
        if (!Directory.Exists(themesPath)) return [];
        return Directory.GetDirectories(themesPath).Select(Path.GetFileName).Where(n => n != null)!;
    }

    public void SetTheme(string themeName) => _activeTheme = themeName;
}
