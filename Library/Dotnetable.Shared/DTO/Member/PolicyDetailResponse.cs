namespace Dotnetable.Shared.DTO.Member;

public class PolicyDetailResponse
{
    public int PolicyID { get; set; }
    public string Title { get; set; }
    public bool Active { get; set; }

    public Public.ErrorExceptionResponse ErrorException { get; set; }
}
