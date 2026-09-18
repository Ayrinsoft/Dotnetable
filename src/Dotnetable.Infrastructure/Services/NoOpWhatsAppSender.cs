using Dotnetable.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Default WhatsApp sender until a provider factory + per-website settings exist.
/// Never sends; logs only. A real implementation must resolve the website's own gateway and fall
/// back to the master website's, exactly like email (see <see cref="IWhatsAppSender"/>).
/// </summary>
public class NoOpWhatsAppSender : IWhatsAppSender
{
    private readonly ILogger<NoOpWhatsAppSender> _logger;

    public NoOpWhatsAppSender(ILogger<NoOpWhatsAppSender> logger) => _logger = logger;

    public Task<bool> IsConfiguredAsync(int websiteId, CancellationToken ct = default) => Task.FromResult(false);

    public Task<bool> SendAsync(int websiteId, string countryCode, string cellphone, string message, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "WhatsApp (not sent — no gateway configured) for website {WebsiteId} to +{Code}{Phone}: {Message}",
            websiteId, countryCode, cellphone, message);
        return Task.FromResult(false);
    }
}
