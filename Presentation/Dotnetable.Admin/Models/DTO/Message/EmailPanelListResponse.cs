using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Admin.Models.DTO.Message;

public class EmailPanelListResponse
{
    public int DatabaseRecords { get; set; }
    public List<EmailSettingDetail> EmailSettings { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class EmailSettingDetail
    {
        public int EmailSettingID { get; set; }
        public string EmailAddress { get; set; }
        public string EmailPassword { get; set; }
        public int SMTPPort { get; set; }
        public string EmailName { get; set; }
        public string MailServer { get; set; }
        public byte EMailType { get; set; }
        public bool Active { get; set; }
        public bool EnableSSL { get; set; }
        public bool DefaultEmail { get; set; }
    }
}
