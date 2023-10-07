using System.ComponentModel.DataAnnotations;

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
        /// "Server=localhost;Port=5432;Database=Dotnetable;User Id=username;Password=password;" //POSTGRESQL
        /// "server=localhost;port=3307;uid=username;pwd=password;database=Dotnetable;" //MYSQL / MARIADB
        /// "Server=localhost;Database=Dotnetable;User Id=username;Password=password;TrustServerCertificate=True;" //MSSQL
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
        [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_AppName_Required))]
        [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
        [MinLength(2, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_2))]
        public string AppName { get; set; }

        public string RecaptchaPublicKey { get; set; }
        public string RecaptchaPrivateKey { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_TokenHash_Required))]
        [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
        [MinLength(2, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_2))]
        public string TokenHash { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_ClientHash_Required))]
        [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
        [MinLength(2, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_2))]
        public string ClientHash { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_LanguageCode_Required))]
        [StringLength(2, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_2))]
        [MinLength(2, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_2))]
        public string DefaultLanguageCode { get; set; }

        public string ApplicationVersion { get; set; } = "1.0.0";

        public List<string> WhiteListWebsites { get; set; } = new();
    }

}
