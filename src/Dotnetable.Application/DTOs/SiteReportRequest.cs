namespace Dotnetable.Application.DTOs;

/// <summary>Payload for a visitor report about a bug or content on the public site.</summary>
public sealed class SiteReportRequest
{
    public string ReporterName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary><see cref="Domain.Enums.SiteReportKind"/> as a number.</summary>
    public byte Kind { get; set; }

    public string Subject { get; set; } = string.Empty;
    public string MessageBody { get; set; } = string.Empty;

    /// <summary>Page the visitor was on. Absolute http(s) URL or a site path starting with <c>/</c>.</summary>
    public string? PageUrl { get; set; }

    public string? CaptchaToken { get; set; }
    public string? CaptchaAnswer { get; set; }

    /// <summary>Honeypot. Must stay empty.</summary>
    public string? Website { get; set; }
}
