namespace Dotnetable.Application.DTOs;

/// <summary>Payload for a visitor newsletter signup.</summary>
public sealed class SubscribeRequest
{
    public string Email { get; set; } = string.Empty;

    /// <summary>Honeypot field: must stay empty.</summary>
    public string? Website { get; set; }
}
