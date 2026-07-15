using Dotnetable.Application.Interfaces;

namespace Dotnetable.Admin.Localization;

public sealed class AuthLanguageResolver(IWebsiteService websiteService) : IAuthLanguageResolver
{
    public string ResolveFromCookie(string? cookieValue) =>
        SupportedLanguages.IsSupported(cookieValue) ? cookieValue! : string.Empty;

    public async Task<string> ResolveAsync(string? cookieValue, int websiteId, CancellationToken ct = default)
    {
        if (SupportedLanguages.IsSupported(cookieValue))
            return cookieValue!;

        var website = await websiteService.GetByIdAsync(websiteId, ct);
        return SupportedLanguages.IsSupported(website?.DefaultLanguageCode)
            ? website!.DefaultLanguageCode
            : SupportedLanguages.All[0].Code;
    }
}
