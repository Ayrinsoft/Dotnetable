using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

/// <summary>An editable email template, resolved for a given website (own row or master fallback).</summary>
public class EmailTemplateInfo
{
    public int EmailTemplateID { get; set; }
    public int WebsiteID { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public EmailAccountType AccountType { get; set; } = EmailAccountType.NoReply;
    public bool Active { get; set; } = true;

    /// <summary>True when this row belongs to the requested website; false when it's the master fallback.</summary>
    public bool IsOverride { get; set; }
}
