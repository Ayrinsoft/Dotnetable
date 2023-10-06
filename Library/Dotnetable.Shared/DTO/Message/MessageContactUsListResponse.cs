namespace Dotnetable.Shared.DTO.Message;

public class MessageContactUsListResponse
{
    public int DatabaseRecords { get; set; }
    public List<ContactMessage> ContactUsMessages { get; set; }
    public Public.ErrorExceptionResponse ErrorException { get; set; }

    public class ContactMessage
    {
        public int ContactUsMessagesID { get; set; }
        public string SenderName { get; set; }
        public string EmailAddress { get; set; }
        public string CellphoneNumber { get; set; }
        public string MessageSubject { get; set; }
        public string MessageBody { get; set; }
        public bool Archive { get; set; }
        public DateTime LogTime { get; set; }
        public string SenderIPAddress { get; set; }
    }

}
