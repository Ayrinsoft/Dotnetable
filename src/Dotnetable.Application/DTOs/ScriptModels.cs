namespace Dotnetable.Application.DTOs;

/// <summary>An active website script the storefront injects. Position: 0 head, 1 body start, 2 body end.
/// LoadCondition: 0 always, 1 signed-in visitors, 2 anonymous visitors.</summary>
public sealed class PublicScriptDto
{
    public byte Position { get; init; }
    public byte LoadCondition { get; init; }
    public byte? Priority { get; init; }
    public string Content { get; init; } = string.Empty;
}
