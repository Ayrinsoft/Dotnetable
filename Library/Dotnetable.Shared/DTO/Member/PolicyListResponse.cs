namespace Dotnetable.Shared.DTO.Member;

public class PolicyListResponse
{
    public int DatabaseRecords { get; set; }
    public List<PolicyDetail> Policies { get; set; }
    public Public.ErrorExceptionResponse ErrorException { get; set; }

    public class PolicyDetail
    {
        public int PolicyID { get; set; }
        public string Title { get; set; }
        public bool Active { get; set; }
    }
}
