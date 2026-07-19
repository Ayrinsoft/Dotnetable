using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class EmailAccount
{
    public int EmailAccountID { get; set; }

    public int WebsiteID { get; set; }

    public byte AccountType { get; set; }

    public string Name { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string MailServer { get; set; } = null!;

    public int SMTPPort { get; set; }

    public bool EnableSSL { get; set; }

    public string MailName { get; set; } = null!;

    public bool IsDefault { get; set; }

    public bool Active { get; set; }

    public virtual Website Website { get; set; } = null!;
}
