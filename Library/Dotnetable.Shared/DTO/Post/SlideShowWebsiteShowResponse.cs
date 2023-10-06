namespace Dotnetable.Shared.DTO.Post;

public class SlideShowWebsiteShowResponse
{
    public List<SlideShowDetail> SlideShows { get; set; }
    public Public.ErrorExceptionResponse ErrorException { get; set; }

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
