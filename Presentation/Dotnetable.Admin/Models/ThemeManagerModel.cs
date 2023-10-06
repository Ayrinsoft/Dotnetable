using MudBlazor;

namespace Dotnetable.Admin.Models;

public class ThemeManagerModel
{
    public bool IsDarkMode { get; set; }
    public string PrimaryColor { get; set; }
    public MaxWidth MaxWidthPage { get; set; }
    public string LanguageCode { get; set; } = "EN";
    public string CurrentCulture { get; set; } = "en-US";
}