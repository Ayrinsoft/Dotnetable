using Dotnetable.Application.Interfaces;

namespace Dotnetable.Admin.Localization;

public sealed class AuthLanguageResolver(IWebsiteService websiteService, ILanguageService languageService) : IAuthLanguageResolver
{
    public async Task<string> ResolveAsync(string? cookieValue, int websiteId, CancellationToken ct = default)
    {
        var options = (await languageService.GetActiveCatalogAsync(ct)).Select(l => l.ToOption()).ToList();

        if (options.IsSupported(cookieValue))
            return cookieValue!;

        var website = await websiteService.GetByIdAsync(websiteId, ct);
        return options.IsSupported(website?.DefaultLanguageCode)
            ? website!.DefaultLanguageCode
            : options.Get(null).Code;
    }
}
