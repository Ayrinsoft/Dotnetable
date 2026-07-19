namespace Dotnetable.Domain.Entities;

/// <summary>
/// Localized subject/body for an <see cref="EmailTemplate"/>. The parent row holds the website's
/// default-language content; non-default languages live here and fall back to the parent when blank.
/// </summary>
public partial class EmailTemplateTranslation
{
    public int EmailTemplateTranslationID { get; set; }

    public int EmailTemplateID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string HtmlBody { get; set; } = null!;

    public virtual EmailTemplate EmailTemplate { get; set; } = null!;
}
