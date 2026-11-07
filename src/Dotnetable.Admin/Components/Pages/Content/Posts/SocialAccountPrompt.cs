using Dotnetable.Admin.Localization;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dotnetable.Admin.Components.Pages.Content.Posts;

/// <summary>Asks to open Website → Social publishing when no network account is saved.</summary>
public static class SocialAccountPrompt
{
    public const string Path = "/website/social-accounts";

    public static async Task OfferSetupAsync(IDialogService dialogs, IPageLocalizer localizer, NavigationManager nav)
    {
        var fa = localizer.LanguageCode.StartsWith("fa", StringComparison.OrdinalIgnoreCase);
        var go = await dialogs.ShowMessageBoxAsync(
            fa ? "حساب شبکه‌ها ثبت نشده" : localizer["posts.social_missing_title", "Social accounts are not set"],
            fa
                ? "برای این وب‌سایت اینستاگرام، تلگرام و X ذخیره نشده است. اگر می‌خواهید اطلاعات را پر کنید، به صفحه مدیریت انتشار در شبکه‌ها بروید."
                : localizer["posts.social_missing_body", "Instagram, Telegram, and X have no saved account for this website. Open Social publishing under Website if you want to fill them in."],
            yesText: fa ? "برو به صفحه مدیریت" : localizer["posts.social_missing_go", "Open the page"],
            cancelText: localizer["cancel", "Cancel"]);
        if (go == true)
            nav.NavigateTo(Path);
    }
}
