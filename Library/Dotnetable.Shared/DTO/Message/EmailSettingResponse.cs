namespace Dotnetable.Shared.DTO.Message
{
    public class EmailSettingResponse
    {
        public string EmailAddress { get; set; }
        public int PortNumber { get; set; }
        public string MailServer { get; set; }
        public bool EnableSSL { get; set; }
        public string Password { get; set; }
        public string MailName { get; set; }
    }
}
