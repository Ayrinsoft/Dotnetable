using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Web.Models;

/// <summary>View model for the public Contact page: CMS page content plus the contact-form fields.</summary>
public class ContactPageViewModel
{
    public string PageTitle { get; set; } = "Contact";
    public string ContentHtml { get; set; } = string.Empty;
    public bool Submitted { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Subject { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;
}
