using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Message
{
   public class EmailSendRequest
    {
        public string EmailAddress { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public int? SenderMemberID { get; set; }
        public EmailType EmailType { get; set; } = EmailType.NOREPLY;
    }
}
