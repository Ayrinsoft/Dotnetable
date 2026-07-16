namespace Dotnetable.Application.DTOs;

/// <summary>A website's own active language, as shown in the front-end language switcher.</summary>
public sealed class LanguageDto
{
    public string Code { get; init; } = string.Empty;
    public string CodeISO { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public bool RTLDesign { get; init; }
}
