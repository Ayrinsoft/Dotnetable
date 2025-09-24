using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Admin.Models.Charts.DTO.Post;

public class SlideShowLanguageListResponse
{
    public List<LanguagesDetail> SlideShowLanguages { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class LanguagesDetail
    {
        public int SlideShowLanguageID { get; set; }
        public string Title { get; set; }
        public string SettingsArray { get; set; }
        public string LanguageCode { get; set; }
    }
}
