using Dotnetable.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Placeholder WhatsApp sender until a real provider is wired. Logs the message in development.
/// </summary>
public class NoOpWhatsAppSender : IWhatsAppSender
{
    private readonly ILogger<NoOpWhatsAppSender> _logger;

    public NoOpWhatsAppSender(ILogger<NoOpWhatsAppSender> logger) => _logger = logger;

    public bool IsConfigured => false;

    public Task SendAsync(string countryCode, string cellphone, string message, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "WhatsApp (not sent — no gateway configured) to +{Code}{Phone}: {Message}",
            countryCode, cellphone, message);
        return Task.CompletedTask;
    }
}
