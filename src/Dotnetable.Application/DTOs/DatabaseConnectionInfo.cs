namespace Dotnetable.Application.DTOs;

/// <summary>Database connection details collected by the Setup page (MariaDB/MySQL).</summary>
public class DatabaseConnectionInfo
{
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 3306;
    public string DatabaseName { get; set; } = "Dotnetable";
    public string Username { get; set; } = "root";
    public string Password { get; set; } = string.Empty;

    /// <summary>Create the database if it does not yet exist on the server.</summary>
    public bool CreateDatabaseIfMissing { get; set; } = true;

    /// <summary>Builds a MySql.Data connection string, optionally without the database (server scope).</summary>
    public string ToConnectionString(bool includeDatabase = true)
    {
        var database = includeDatabase ? $"Database={DatabaseName};" : string.Empty;
        return $"Server={Server};Port={Port};{database}User Id={Username};Password={Password};";
    }
}
