using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Admin.Models.DTO.Member;

public class PolicyDetailResponse
{
    public int PolicyID { get; set; }
    public string Title { get; set; }
    public bool Active { get; set; }

    public ErrorExceptionResponse ErrorException { get; set; }
}
