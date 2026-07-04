namespace Dotnetable.Application.DTOs;

/// <summary>Payload for a visitor-submitted contact form message.</summary>
public sealed class ContactMessageRequest
{
    public string SenderName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string CellphoneNumber { get; set; } = string.Empty;
    public string MessageSubject { get; set; } = string.Empty;
    public string MessageBody { get; set; } = string.Empty;
}
