using System.ComponentModel.DataAnnotations;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetable.Admin.Pages;

public class SetupModel : PageModel
{
    private readonly ISetupService _setupService;

    public SetupModel(ISetupService setupService)
    {
        _setupService = setupService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        // Website
        [Required, StringLength(32)] public string TradeName { get; set; } = string.Empty;
        [Required, StringLength(60)] public string BrandName { get; set; } = string.Empty;
        [Required, StringLength(60)] public string WebsiteAddress { get; set; } = string.Empty;
        [Required, StringLength(30)] public string Manager { get; set; } = string.Empty;
        [Required, StringLength(15)] public string Mobile { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(60)] public string WebsiteEmail { get; set; } = string.Empty;
        [Required, StringLength(2, MinimumLength = 2)] public string DefaultLanguageCode { get; set; } = "en";

        // Administrator
        [Required, StringLength(64)] public string Username { get; set; } = string.Empty;
        [Required, StringLength(256, MinimumLength = 6), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(64)] public string Email { get; set; } = string.Empty;
        [Required, StringLength(64)] public string Givenname { get; set; } = string.Empty;
        [Required, StringLength(64)] public string Surname { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (await _setupService.IsSetupCompletedAsync(ct))
            return RedirectToPage("/Login");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (await _setupService.IsSetupCompletedAsync(ct))
            return RedirectToPage("/Login");

        if (!ModelState.IsValid) return Page();

        var request = new SetupRequest
        {
            TradeName = Input.TradeName,
            BrandName = Input.BrandName,
            WebsiteAddress = Input.WebsiteAddress,
            Manager = Input.Manager,
            Mobile = Input.Mobile,
            WebsiteEmail = Input.WebsiteEmail,
            DefaultLanguageCode = Input.DefaultLanguageCode,
            Username = Input.Username,
            Password = Input.Password,
            Email = Input.Email,
            Givenname = Input.Givenname,
            Surname = Input.Surname,
        };

        try
        {
            await _setupService.CompleteSetupAsync(request, ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Setup failed: {ex.Message}";
            return Page();
        }

        return RedirectToPage("/Login");
    }
}
