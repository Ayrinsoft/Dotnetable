namespace Dotnetable.Shared.DTO.Message;

public class MessageContactUsChangesRequest
{
    public int CurrentMemberID { get; set; }
    public int ContactUsMessageID { get; set; }
    public bool DeleteItem { get; set; }
}
