namespace Dotnetable.Shared.DTO.Post;

public class PostDetailResponse
{
    public int PostID { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Body { get; set; }
    public int PostCategoryID { get; set; }
    public string Tags { get; set; }
    public string MetaKeywords { get; set; }
    public string MetaDescription { get; set; }
    public string LanguageCode { get; set; }
    public Guid? MainImage { get; set; }
    public bool Active { get; set; }
    public List<PostFiles> FileList { get; set; }

    public Public.ErrorExceptionResponse ErrorException { get; set; }

    public class PostFiles
    {
        public Guid FileCode { get; set; }
        public string FileName { get; set; }
    }
}
