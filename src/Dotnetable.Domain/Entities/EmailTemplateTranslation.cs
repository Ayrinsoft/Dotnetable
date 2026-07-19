using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class EmailTemplateTranslation
{
    public int EmailTemplateTranslationID { get; set; }

    public int EmailTemplateID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string HtmlBody { get; set; } = null!;

    public virtual EmailTemplate EmailTemplate { get; set; } = null!;
}
