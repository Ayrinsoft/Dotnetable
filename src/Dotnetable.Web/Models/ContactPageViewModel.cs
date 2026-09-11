using Dotnetable.Application.DTOs;

namespace Dotnetable.Web.Models;

/// <summary>View model for the public Contact page: CMS page content plus the admin-managed contact
/// details (phones, emails, addresses, hours, ...). The form itself is submitted via JS straight to
/// <c>Home/ContactSubmit</c>, so no form-field properties are needed here.</summary>
public class ContactPageViewModel
{
    public string PageTitle { get; set; } = "Contact";
    public string ContentHtml { get; set; } = string.Empty;
    public IReadOnlyList<ContactInfoDto> ContactInfos { get; set; } = Array.Empty<ContactInfoDto>();
}
