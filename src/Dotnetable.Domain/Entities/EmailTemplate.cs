using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// An editable subject/HTML pair for one well-known outgoing email (see
/// <c>Dotnetable.Application.Email.EmailTemplateKeys</c>). Every website is independent, but a row is
/// only created once a website customizes that template — until then, sending falls back to the row
/// owned by the master website (<c>AppConstants.MasterWebsiteId</c>), which is seeded on first run.
/// </summary>
public partial class EmailTemplate
{
    public int EmailTemplateID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Well-known identifier, e.g. "AdminForgotPassword". Unique per website.</summary>
    public string TemplateKey { get; set; } = null!;

    /// <summary>Admin-facing name shown in the templates list.</summary>
    public string Name { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string HtmlBody { get; set; } = null!;

    /// <summary>Preferred sender — see <see cref="Enums.EmailAccountType"/>.</summary>
    public byte AccountType { get; set; }

    public bool Active { get; set; }

    public virtual Website Website { get; set; } = null!;
}
