using Asp.Versioning;
using Dotnetable.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// One-time bootstrap so Admin's Setup wizard can hand this API the same <c>Internal:SyncSecret</c>
/// it just generated for itself, without anyone copying the value between servers by hand.
/// </summary>
[ApiController]
[ApiVersionNeutral]
[Route("api/internal")]
public class InternalController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<InternalController> _logger;

    public InternalController(IConfiguration configuration, IHostEnvironment environment, ILogger<InternalController> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Accepts a shared secret only while this API's own <c>Internal:SyncSecret</c> is still unset or
    /// the placeholder committed to source control. Once a real secret is on file this permanently
    /// refuses, so the endpoint cannot be used to hijack an already-configured deployment — it exists
    /// purely to save a manual copy/paste on first setup.
    /// </summary>
    [HttpPost("bootstrap-sync-secret")]
    public IActionResult BootstrapSyncSecret([FromBody] BootstrapSyncSecretRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Secret) || request.Secret.Length < 16)
            return BadRequest(new { message = "Secret must be at least 16 characters." });

        var current = _configuration["Internal:SyncSecret"];
        var alreadyConfigured = !string.IsNullOrWhiteSpace(current) && !StartupValidation.IsPlaceholderSecret(current);
        if (alreadyConfigured)
            return Conflict(new { message = "Internal:SyncSecret is already configured on this API." });

        if (!StartupValidation.TrySetLocalSetting(_configuration, _environment, "Internal:SyncSecret", request.Secret, out var error))
        {
            _logger.LogWarning("Failed to bootstrap Internal:SyncSecret: {Error}", error);
            return Problem(error);
        }

        _logger.LogInformation("Internal:SyncSecret was bootstrapped from Admin's Setup wizard.");
        return NoContent();
    }
}

public record BootstrapSyncSecretRequest(string Secret);
