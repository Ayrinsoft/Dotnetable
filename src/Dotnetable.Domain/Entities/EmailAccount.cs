using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// One outgoing-mail identity (SMTP credentials + "From" name) belonging to a website. A website can
/// register several — NoReply, Support, Sales, Marketing, Info, or arbitrary Custom ones — so each kind
/// of transactional or marketing email is sent from (and repliable to) the right address.
/// </summary>
public partial class EmailAccount
{
    public int EmailAccountID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>See <see cref="Enums.EmailAccountType"/>.</summary>
    public byte AccountType { get; set; }

    /// <summary>Admin-facing label, e.g. "Support" or a custom name when <see cref="AccountType"/> is Custom.</summary>
    public string Name { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string MailServer { get; set; } = null!;

    public int SMTPPort { get; set; }

    public bool EnableSSL { get; set; }

    /// <summary>Friendly "From" display name.</summary>
    public string MailName { get; set; } = null!;

    /// <summary>Fallback account used when a website/type-specific account isn't found.</summary>
    public bool IsDefault { get; set; }

    public bool Active { get; set; }

    public virtual Website Website { get; set; } = null!;
}
