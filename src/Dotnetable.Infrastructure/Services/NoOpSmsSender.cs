using Dotnetable.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Placeholder SMS sender used until a real gateway is wired up. It never sends anything; it logs
/// the outgoing message so the OTP flow can be exercised end-to-end in development.
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
