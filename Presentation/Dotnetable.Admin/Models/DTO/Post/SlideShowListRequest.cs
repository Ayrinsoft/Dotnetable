using Dotnetable.SharedDTO.p.Public;

namespace Dotnetable.Admin.Models.Charts.DTO.Post;

public class SlideShowListRequest: GridviewRequest
{
    public string Title { get; set; }
    public string PageCode { get; set; }
}
