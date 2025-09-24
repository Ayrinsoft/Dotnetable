using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Admin.Models.DTO.Post;

public class PostListFetchRequest : GridviewRequest
{
    public string Title { get; set; }
}
