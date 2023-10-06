namespace Dotnetable.Shared.DTO.Post;

public class SlideShowListResponse
{
    public int DatabaseRecords { get; set; }
    public List<SlideShowDetail> SlideShows { get; set; }
    public Public.ErrorExceptionResponse ErrorException { get; set; }

    public class SlideShowDetail
    {
        public int SlideShowID { get; set; }
        public string Title { get; set; }
        public string PageCode { get; set; }
        public bool Active { get; set; }
        public string LanguageCode { get; set; }
        public byte Priority { get; set; }
        public Guid FileCode { get; set; }
        public string LanguageCodes { get; set; }
    }
}
