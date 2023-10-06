namespace Dotnetable.Shared.DTO.Post;

public class PostCategoryUpdatePriorityAndParentRequest
{
    public int PostCategoryID { get; set; }
    public short Priority { get; set; }
    public int? ParentID { get; set; }
}
