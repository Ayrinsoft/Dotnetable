using System.ComponentModel.DataAnnotations;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Admin.Pages;

[AllowAnonymous]
public class ForgotPasswordModel : CaptchaPageModel
{
    private readonly IPasswordResetService _resetService;

    public ForgotPasswordModel(IPasswordResetService resetService, IHumanVerificationService human) : base(human)
    {
        _resetService = resetService;
    }

    [BindProperty, Required]
    public string EmailOrUsername { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    private const string GenericSent =
        "If an account matches, a password reset link has been sent to its email address.";

    public IActionResult OnGet()
    {
        PrepareCaptcha();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            PrepareCaptcha();
            return Page();
        }

        if (!await ValidateCaptchaAsync(ct))
            return Page();

        try
        {
            var result = await _resetService.RequestResetAsync(
                EmailOrUsername,
                key => Url.Page("/ResetPassword", pageHandler: null, values: new { key }, protocol: Request.Scheme)!,
                ct);

            if (result == PasswordResetRequestResult.EmailNotConfigured)
            {
                ErrorMessage = "Email sending is not configured. Please contact your administrator.";
                PrepareCaptcha();
                return Page();
            }

            // Sent or MemberNotFound: identical generic response to avoid revealing which accounts exist.
            SuccessMessage = GenericSent;
            return Page();
        }
        catch
        {
            ErrorMessage = "We couldn't send the email right now. Please try again later.";
            PrepareCaptcha();
            return Page();
        }
    }
}
