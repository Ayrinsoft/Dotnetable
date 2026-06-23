using System.ComponentModel.DataAnnotations;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Admin.Pages;

[AllowAnonymous]
public class ResetPasswordModel : CaptchaPageModel
{
    private readonly IPasswordResetService _resetService;

    public ResetPasswordModel(IPasswordResetService resetService, IHumanVerificationService human) : base(human)
    {
        _resetService = resetService;
    }

    [BindProperty(SupportsGet = true)]
    public string Key { get; set; } = string.Empty;

    [BindProperty, Required, StringLength(100, MinimumLength = 6,
        ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    [BindProperty, Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool KeyValid { get; set; }
    public bool Completed { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        KeyValid = await _resetService.IsKeyValidAsync(Key, ct);
        if (KeyValid) PrepareCaptcha();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        KeyValid = await _resetService.IsKeyValidAsync(Key, ct);
        if (!KeyValid) return Page();

        if (!ModelState.IsValid)
        {
            PrepareCaptcha();
            return Page();
        }

        if (!await ValidateCaptchaAsync(ct))
            return Page();

        var ok = await _resetService.ResetPasswordAsync(Key, Password, ct);
        if (!ok)
        {
            KeyValid = false; // key expired/consumed between page load and submit
            return Page();
        }

        Completed = true;
        return Page();
    }
}
