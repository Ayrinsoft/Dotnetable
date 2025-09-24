using Dotnetable.SharedDTO.p.Public;

namespace Dotnetable.Admin.Models.Charts.DTO.Post;

public class SlideShowDetailResponse
{
    public int SlideShowID { get; set; }
    public string Title { get; set; }
    public string LanguageCode { get; set; }
    public string SettingsArray { get; set; }
    public bool Active { get; set; }
    public byte Priority { get; set; }
    public string PageCode { get; set; }
    public Guid FileCode { get; set; }

    public ErrorExceptionResponse ErrorException { get; set; }
}
