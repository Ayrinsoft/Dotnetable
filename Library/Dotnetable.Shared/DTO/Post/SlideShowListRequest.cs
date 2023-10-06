using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Post;

public class SlideShowListRequest: GridviewRequest
{
    public string Title { get; set; }
    public string PageCode { get; set; }
}
