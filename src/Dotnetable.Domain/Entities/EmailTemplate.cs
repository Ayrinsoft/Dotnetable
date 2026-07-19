using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class EmailTemplate
{
    public int EmailTemplateID { get; set; }

    public int WebsiteID { get; set; }

    public string TemplateKey { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string HtmlBody { get; set; } = null!;

    public byte AccountType { get; set; }

    public bool Active { get; set; }

    public virtual ICollection<EmailTemplateTranslation> EmailTemplateTranslations { get; set; } = new List<EmailTemplateTranslation>();

    public virtual Website Website { get; set; } = null!;
}
