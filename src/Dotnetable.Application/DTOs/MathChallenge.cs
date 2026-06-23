namespace Dotnetable.Application.DTOs;

/// <summary>
/// A single "add/subtract two numbers" captcha. The expected answer lives server-side in a
/// short-lived cache keyed by <see cref="Token"/>; the page only ever sees the rendered image.
/// </summary>
public class MathChallenge
{
    /// <summary>Opaque id carried in a hidden form field and used to look up the cached answer.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Inline SVG markup of the question (e.g. "3 + 4 = ?").</summary>
    public string Svg { get; set; } = string.Empty;
}
