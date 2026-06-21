using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class EmailSetting
{
    public int EmailSettingID { get; set; }

    public string EmailAddress { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int SMTPPort { get; set; }

    public bool EnableSSL { get; set; }

    public string MailName { get; set; } = null!;

    public string MailServer { get; set; } = null!;

    public byte EmailTypeID { get; set; }

    public bool DefaultEMail { get; set; }

    public bool Active { get; set; }
}
