namespace Dotnetable.Shared.DTO.Post;

public class PostListFetchResponse
{
    public int DatabaseRecords { get; set; }
    public List<PostDetail> Posts { get; set; }
    public Public.ErrorExceptionResponse ErrorException { get; set; }


    public class PostDetail
    {
        public int PostID { get; set; }
        public string PostCategoryName { get; set; }
        public int PostCategoryID { get; set; }
        public string ModifierName { get; set; }
        public DateTime ModifyDate { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Tags { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public bool Active { get; set; }
        public string LanguageCodes { get; set; }
    }
}
