namespace Dotnetable.Application.DTOs;

public record ConnectionTestResult(bool Success, string Message)
{
    public static ConnectionTestResult Ok(string message = "Connection succeeded.") => new(true, message);
    public static ConnectionTestResult Fail(string message) => new(false, message);
}
