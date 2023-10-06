namespace Dotnetable.Shared.DTO.Member;

public class SubscribeListResponse
{
    public int DatabaseRecords { get; set; } = 0;
    public List<SubscribeDetail> SubscribedList { get; set; }
    public Public.ErrorExceptionResponse ErrorException { get; set; }

    public class SubscribeDetail
    {
        public int EmailSubscribeID { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }
        public DateTime LogTime { get; set; }
        public int? MemberID { get; set; }
    }
}
