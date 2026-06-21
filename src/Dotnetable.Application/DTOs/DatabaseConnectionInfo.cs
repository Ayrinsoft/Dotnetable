namespace Dotnetable.Application.DTOs;

/// <summary>Database connection details collected by the Setup page. Provider-agnostic.</summary>
public class DatabaseConnectionInfo
{
    /// <summary>SqlServer (default/primary), PostgreSQL, MySQL or MariaDB.</summary>
    public string Provider { get; set; } = "SqlServer";

    public string Server { get; set; } = ".";
    public int Port { get; set; } = 1433;
    public string DatabaseName { get; set; } = "Dotnetable";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>SQL Server only: use Windows authentication instead of a username/password.</summary>
    public bool IntegratedSecurity { get; set; } = true;

    /// <summary>SQL Server only: accept the server certificate without validation.</summary>
    public bool TrustServerCertificate { get; set; } = true;

    /// <summary>Create the database if it does not yet exist on the server.</summary>
    public bool CreateDatabaseIfMissing { get; set; } = true;
}
