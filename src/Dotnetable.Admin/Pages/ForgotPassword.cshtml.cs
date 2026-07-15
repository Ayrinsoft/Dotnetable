using System.ComponentModel.DataAnnotations;
using Dotnetable.Admin.Localization;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Admin.Pages;

[AllowAnonymous]
public class ForgotPasswordModel : CaptchaPageModel
{
    private readonly IPasswordResetService _resetService;

    public ForgotPasswordModel(IPasswordResetService resetService, IHumanVerificationService human,
        IAuthLanguageResolver langResolver, ILanguageService languageService) : base(human, langResolver, languageService)
    {
        _resetService = resetService;
    }

    [BindProperty, Required]
    public string EmailOrUsername { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        await ResolveLanguageAsync(ct);
        PrepareCaptcha();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await ResolveLanguageAsync(ct);

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
                ErrorMessage = S.EmailNotConfigured;
                PrepareCaptcha();
                return Page();
            }

            // Sent or MemberNotFound: identical generic response to avoid revealing which accounts exist.
            SuccessMessage = S.GenericSent;
            return Page();
        }
        catch
        {
            ErrorMessage = S.TryAgainLater;
            PrepareCaptcha();
            return Page();
        }
    }
}
