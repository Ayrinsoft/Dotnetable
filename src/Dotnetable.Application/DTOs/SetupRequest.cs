namespace Dotnetable.Application.DTOs;

/// <summary>
/// Initial data collected by the first-run setup form: the master website plus its administrator member.
/// </summary>
public class SetupRequest
{
    // Database connection (tested, optionally created, then persisted)
    public DatabaseConnectionInfo Database { get; set; } = new();

    // Website
    public string TradeName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string WebsiteAddress { get; set; } = string.Empty;
    public string Manager { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string WebsiteEmail { get; set; } = string.Empty;
    public string DefaultLanguageCode { get; set; } = "en";

    // Administrator member
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Givenname { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
}
