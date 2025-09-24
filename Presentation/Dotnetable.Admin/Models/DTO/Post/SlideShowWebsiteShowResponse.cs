using Dotnetable.SharedDTO.p.Public;

namespace Dotnetable.Admin.Models.Charts.DTO.Post;

public class SlideShowWebsiteShowResponse
{
    public List<SlideShowDetail> SlideShows { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class SlideShowDetail
    {
        public int SlideShowID { get; set; }
        public Guid FileCode { get; set; }
        public byte Priority { get; set; }
        public string LanguageCode { get; set; }
        public string SettingArray { get; set; }
        public string Title { get; set; }
    }
}
