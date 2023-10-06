namespace Dotnetable.Shared.DTO.Website;

public class AdminPanelAppSettingsResponse
{
    public ConnectionStringModel ConnectionStrings { get; set; }
    public DBSettingsModel DBSettings { get; set; }
    public AdminPanelSettingsModel AdminPanelSettings { get; set; }
    public string InsertDataMode { get; set; } = "CREATE_DB";
    public string AllowedHosts { get; set; } = "*";

    public class ConnectionStringModel
    {
        /// <summary>
        /// "Server=localhost;Port=5432;Database=Titan;User Id=ali;Password=123ali;" //POSTGRESQL
        /// "server=localhost;port=3307;uid=root;pwd=123ali;database=Titan;" //MYSQL / MARIADB
        /// "Server=ayrin-server;Database=Titan;User Id=ali;Password=123ali;TrustServerCertificate=True;" //MSSQL
        /// </summary>
        public string DotnetableConnection { get; set; }
    }

    public class DBSettingsModel
    {
        /// <summary>
        /// Dotnetable.Shared.DTO.Public.DatabaseType
        /// </summary>
        public string DBType { get; set; }

        /// <summary>
        /// "10.11.2-GA" //Only MARIADB
        /// </summary>
        public string Version { get; set; }
    }

    public class AdminPanelSettingsModel
    {
        public string AppName { get; set; }
        public string RecaptchaPublicKey { get; set; }
        public string RecaptchaPrivateKey { get; set; }
        public string TokenHash { get; set; }
        public string ClientHash { get; set; }
        public string DefaultLanguageCode { get; set; }
        public string ApplicationVersion { get; set; } = "1.0.0";
        public List<string> WhiteListWebsites { get; set; } = new();
    }

}
