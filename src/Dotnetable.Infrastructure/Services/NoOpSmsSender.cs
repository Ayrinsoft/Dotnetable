using Dotnetable.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Default SMS sender until a provider factory + per-website settings exist.
/// Never sends; logs only so OTP/shipment flows can run in development.
/// </summary>
public class NoOpSmsSender : ISmsSender
{
    private readonly ILogger<NoOpSmsSender> _logger;

    public NoOpSmsSender(ILogger<NoOpSmsSender> logger) => _logger = logger;

    public bool IsConfigured => false;

    public Task SendAsync(string countryCode, string cellphone, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("SMS (not sent — no gateway configured) to +{Code}{Phone}: {Message}",
            countryCode, cellphone, message);
        return Task.CompletedTask;
    }
}
