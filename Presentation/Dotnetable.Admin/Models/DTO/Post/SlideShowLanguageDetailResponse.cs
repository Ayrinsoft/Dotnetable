using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Admin.Models.DTO.Post;

public class SlideShowLanguageDetailResponse
{
    public int SlideShowID { get; set; }
    public int SlideShowLanguageID { get; set; }
    public string LanguageCode { get; set; }
    public string Title { get; set; }
    public string SettingsArray { get; set; }

    public ErrorExceptionResponse ErrorException { get; set; }
}
