using Dotnetable.Shared.DTO.Public;
using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Message;

public class EmailPanelUpdateRequest
{
    public int CurrentMemberID { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_EmailSettingID_Required))]
    [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Public_Int_Length))]
    public int EmailSettingID { get; set; }

    public string EmailAddress { get; set; }
    public string EmailPassword { get; set; }
    public int? SMTPPort { get; set; }
    public bool? EnableSSL { get; set; }
    public bool? DefaultEmail { get; set; }
    public string EmailName { get; set; }
    public string MailServer { get; set; }
    public EmailType? EMailType { get; set; }
}
