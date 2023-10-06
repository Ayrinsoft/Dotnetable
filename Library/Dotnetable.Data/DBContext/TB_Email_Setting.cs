using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Email_Setting
{
    public int EmailSettingID { get; set; }

    public string EmailAddress { get; set; }

    public string Password { get; set; }

    public int SMTPPort { get; set; }

    public bool EnableSSL { get; set; }

    public string MailName { get; set; }

    public string MailServer { get; set; }

    public byte EmailTypeID { get; set; }

    public bool DefaultEMail { get; set; }

    public bool Active { get; set; }

    public virtual TB_Email_Type EmailType { get; set; }
}
