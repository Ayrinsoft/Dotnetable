namespace Dotnetable.Web.Models;

/// <summary>
/// Everything the error page is allowed to show a visitor. Deliberately not the exception: its
/// message routinely carries connection strings, file paths and SQL, none of which belong on a
/// public page. <see cref="RequestId"/> is the bridge — the visitor can quote it and the operator
/// can find the matching line in the log.
/// </summary>
public sealed class ErrorViewModel
{
    public string? RequestId { get; init; }

    public int StatusCode { get; init; } = 500;

    public bool ShowRequestId => !string.IsNullOrWhiteSpace(RequestId);

    /// <summary>Short headline for the status, so one view serves 404, 403 and 500.</summary>
    public string Title => StatusCode switch
    {
        404 => "Page not found",
        403 => "Access denied",
        429 => "Too many requests",
        503 => "Temporarily unavailable",
        _ => "Something went wrong",
    };

    public string Message => StatusCode switch
    {
        404 => "The page you are looking for does not exist, or has moved.",
        403 => "You do not have permission to view this page.",
        429 => "You have made too many requests. Please wait a moment and try again.",
        503 => "The site is temporarily unavailable. Please try again shortly.",
        _ => "An unexpected error occurred. The problem has been logged and we are looking into it.",
    };
}
