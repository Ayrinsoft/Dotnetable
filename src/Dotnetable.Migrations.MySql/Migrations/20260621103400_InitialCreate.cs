using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ContactUsMessage",
                columns: table => new
                {
                    ContactUsMessagesID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SenderName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    EmailAddress = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    CellphoneNumber = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    MessageSubject = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false),
                    MessageBody = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false),
                    Archive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LogTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    SenderIPAddress = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactUsMessage", x => x.ContactUsMessagesID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    CountryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CountryCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "varchar(42)", maxLength: 42, nullable: false),
                    PhonePerfix = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.CountryID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailSetting",
                columns: table => new
                {
                    EmailSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    EmailAddress = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Password = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    SMTPPort = table.Column<int>(type: "int", nullable: false),
                    EnableSSL = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MailName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    MailServer = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    EmailTypeID = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DefaultEMail = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSetting", x => x.EmailSettingID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LoginTry",
                columns: table => new
                {
                    LoginTryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    LogTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsSuccess = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TryIP = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginTry", x => x.LoginTryID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Policy",
                columns: table => new
                {
                    PolicyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policy", x => x.PolicyID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RoleKey = table.Column<string>(type: "varchar(42)", unicode: false, maxLength: 42, nullable: false),
                    Description = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CountryTranslation",
                columns: table => new
                {
                    CountryTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "varchar(42)", maxLength: 42, nullable: false),
                    CountryID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryTranslation", x => x.CountryTranslationID);
                    table.ForeignKey(
                        name: "FK_CountryTranslation_Country",
                        column: x => x.CountryID,
                        principalTable: "Country",
                        principalColumn: "CountryID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "State",
                columns: table => new
                {
                    StateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CountryID = table.Column<int>(type: "int", nullable: false),
                    Tile = table.Column<string>(type: "varchar(48)", maxLength: 48, nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_State", x => x.StateID);
                    table.ForeignKey(
                        name: "FK_State_Country",
                        column: x => x.CountryID,
                        principalTable: "Country",
                        principalColumn: "CountryID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PolicyRole",
                columns: table => new
                {
                    PolicyRoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PolicyID = table.Column<int>(type: "int", nullable: false),
                    RoleID = table.Column<short>(type: "smallint", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyRole", x => x.PolicyRoleID);
                    table.ForeignKey(
                        name: "FK_PolicyRole_Policy",
                        column: x => x.PolicyID,
                        principalTable: "Policy",
                        principalColumn: "PolicyID");
                    table.ForeignKey(
                        name: "FK_PolicyRole_Role",
                        column: x => x.RoleID,
                        principalTable: "Role",
                        principalColumn: "RoleID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "City",
                columns: table => new
                {
                    CityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CountryID = table.Column<int>(type: "int", nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "varchar(48)", maxLength: 48, nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Latitude = table.Column<double>(type: "double", nullable: true),
                    Longitude = table.Column<double>(type: "double", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City", x => x.CityID);
                    table.ForeignKey(
                        name: "FK_City_Country",
                        column: x => x.CountryID,
                        principalTable: "Country",
                        principalColumn: "CountryID");
                    table.ForeignKey(
                        name: "FK_City_State",
                        column: x => x.StateID,
                        principalTable: "State",
                        principalColumn: "StateID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StateTranslation",
                columns: table => new
                {
                    StateTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Tile = table.Column<string>(type: "varchar(48)", maxLength: 48, nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateTranslation", x => x.StateTranslationID);
                    table.ForeignKey(
                        name: "FK_StateTranslation_State",
                        column: x => x.StateID,
                        principalTable: "State",
                        principalColumn: "StateID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CityTranslation",
                columns: table => new
                {
                    CityTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CityID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "varchar(48)", maxLength: 48, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityTranslation", x => x.CityTranslationID);
                    table.ForeignKey(
                        name: "FK_City_Translation_City",
                        column: x => x.CityID,
                        principalTable: "City",
                        principalColumn: "CityID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailSubscribe",
                columns: table => new
                {
                    EmailSubscribeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    LogTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MemberID = table.Column<int>(type: "int", nullable: true),
                    Approved = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSubscribe", x => x.EmailSubscribeID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FileRecord",
                columns: table => new
                {
                    FileRecordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteStorageSettingsID = table.Column<int>(type: "int", nullable: false),
                    StorageProvider = table.Column<short>(type: "smallint", nullable: false),
                    StoragePath = table.Column<string>(type: "varchar(350)", maxLength: 350, nullable: true),
                    CNDUrl = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true),
                    OriginalFileName = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    StoredFileName = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    MimeType = table.Column<string>(type: "varchar(74)", unicode: false, maxLength: 74, nullable: false),
                    FileSizeKB = table.Column<int>(type: "int", nullable: false),
                    MetadataJSON = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    AltText = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    Title = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ThumbnailStorage = table.Column<string>(type: "varchar(350)", maxLength: 350, nullable: true),
                    ThumbnailCDN = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true),
                    FileCategory = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CDNFileCode = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileRecord", x => x.FileRecordID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Website",
                columns: table => new
                {
                    WebsiteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    TradeName = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    WebsiteAddress = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    AuthCode = table.Column<Guid>(type: "char(36)", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Manager = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Mobile = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    RegisterDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AllowAllIP = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DefaultLanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    WebsiteType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    BrandName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false, comment: "show in title of pages"),
                    LogoFileID = table.Column<int>(type: "int", nullable: true),
                    FaveIconFileID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Website", x => x.WebsiteID);
                    table.ForeignKey(
                        name: "FK_Website_FileRecord",
                        column: x => x.LogoFileID,
                        principalTable: "FileRecord",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_Website_FileRecord1",
                        column: x => x.FaveIconFileID,
                        principalTable: "FileRecord",
                        principalColumn: "FileRecordID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Language",
                columns: table => new
                {
                    LangaugeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    LanguageCodeISO = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RTLDesign = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WebsiteID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Language", x => x.LangaugeID);
                    table.ForeignKey(
                        name: "FK_Language_Website",
                        column: x => x.WebsiteID,
                        principalTable: "Website",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Member",
                columns: table => new
                {
                    MemberID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Username = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Password = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    CellphoneNumber = table.Column<string>(type: "varchar(12)", unicode: false, maxLength: 12, nullable: false),
                    CountryCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    RegisterDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Givenname = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Surname = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    AvatarID = table.Column<Guid>(type: "char(36)", nullable: true),
                    HashKey = table.Column<Guid>(type: "char(36)", nullable: false),
                    PolicyID = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    WebsiteID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Member", x => x.MemberID);
                    table.ForeignKey(
                        name: "FK_Member_Policy",
                        column: x => x.PolicyID,
                        principalTable: "Policy",
                        principalColumn: "PolicyID");
                    table.ForeignKey(
                        name: "FK_Member_Website",
                        column: x => x.WebsiteID,
                        principalTable: "Website",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteIP",
                columns: table => new
                {
                    WebsiteIPID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    StartIP = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    EndIP = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    CidrPrefix = table.Column<int>(type: "int", nullable: true),
                    Label = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteIP", x => x.WebsiteIPID);
                    table.ForeignKey(
                        name: "FK_WebsiteIP_Website",
                        column: x => x.WebsiteID,
                        principalTable: "Website",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteScript",
                columns: table => new
                {
                    WebsiteScriptID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    RawContent = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false),
                    ScriptPosition = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ScriptLoadCondition = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    LogTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Priority = table.Column<byte>(type: "tinyint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteScript", x => x.WebsiteScriptID);
                    table.ForeignKey(
                        name: "FK_WebsiteScript_Website",
                        column: x => x.WebsiteID,
                        principalTable: "Website",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteSeoSetting",
                columns: table => new
                {
                    WebsiteSeoSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    DefaultMetaTitle = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true, comment: "Page | {SiteName}"),
                    TitleSeparator = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: true, defaultValue: " | \" or \" - \" or \" › "),
                    DefaultMetaDescription = table.Column<string>(type: "varchar(158)", maxLength: 158, nullable: true),
                    SitemapEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RobotsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CustomRobotsTxt = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteSeoSetting", x => x.WebsiteSeoSettingID);
                    table.ForeignKey(
                        name: "FK_WebsiteSeoSetting_Website",
                        column: x => x.WebsiteID,
                        principalTable: "Website",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteSocialLink",
                columns: table => new
                {
                    WebsiteSocialLinkID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    SocialType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SocialName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    SocialIcon = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    UrlAddress = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteSocialLink", x => x.WebsiteSocialLinkID);
                    table.ForeignKey(
                        name: "FK_WebsiteSocialLink_Website",
                        column: x => x.WebsiteID,
                        principalTable: "Website",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebstieStorageSettings",
                columns: table => new
                {
                    WebsiteStorageSettingsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    StorageProvider = table.Column<short>(type: "smallint", nullable: false),
                    StorageSettingsJSON = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaxFileSizeKB = table.Column<long>(type: "bigint", nullable: false),
                    AllowedExtensions = table.Column<string>(type: "varchar(710)", unicode: false, maxLength: 710, nullable: true),
                    AutoGenerateThumbnails = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebstieStorageSettings", x => x.WebsiteStorageSettingsID);
                    table.ForeignKey(
                        name: "FK_WebstieStorageSettings_Website",
                        column: x => x.WebsiteID,
                        principalTable: "Website",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LocalizationKey",
                columns: table => new
                {
                    LocalizationKeyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ItemKey = table.Column<string>(type: "varchar(72)", unicode: false, maxLength: 72, nullable: false),
                    DefaultValue = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    WebsiteID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationKey", x => x.LocalizationKeyID);
                    table.ForeignKey(
                        name: "FK_LocalizationKey_Language",
                        column: x => x.WebsiteID,
                        principalTable: "Language",
                        principalColumn: "LangaugeID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MemberForgetPassword",
                columns: table => new
                {
                    MemberForgetPasswordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ForgetKey = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    MemberID = table.Column<int>(type: "int", nullable: false),
                    LogTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberForgetPassword", x => x.MemberForgetPasswordID);
                    table.ForeignKey(
                        name: "FK_MemberForgetPassword_Member",
                        column: x => x.MemberID,
                        principalTable: "Member",
                        principalColumn: "MemberID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LocalizationValue",
                columns: table => new
                {
                    LocalizationValueID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    LocalizationKeyID = table.Column<int>(type: "int", nullable: false),
                    ItemValue = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationValue", x => x.LocalizationValueID);
                    table.ForeignKey(
                        name: "FK_LocalizationValue_LocalizationKey",
                        column: x => x.LocalizationKeyID,
                        principalTable: "LocalizationKey",
                        principalColumn: "LocalizationKeyID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_City_CountryID",
                table: "City",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_City_StateID",
                table: "City",
                column: "StateID");

            migrationBuilder.CreateIndex(
                name: "IX_CityTranslation_CityID",
                table: "CityTranslation",
                column: "CityID");

            migrationBuilder.CreateIndex(
                name: "IX_CountryTranslation_CountryID",
                table: "CountryTranslation",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSubscribe_MemberID",
                table: "EmailSubscribe",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecord_WebsiteStorageSettingsID",
                table: "FileRecord",
                column: "WebsiteStorageSettingsID");

            migrationBuilder.CreateIndex(
                name: "IX_Language_WebsiteID",
                table: "Language",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationKey_WebsiteID",
                table: "LocalizationKey",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationValue_LocalizationKeyID",
                table: "LocalizationValue",
                column: "LocalizationKeyID");

            migrationBuilder.CreateIndex(
                name: "IX_Member_PolicyID",
                table: "Member",
                column: "PolicyID");

            migrationBuilder.CreateIndex(
                name: "IX_Member_WebsiteID",
                table: "Member",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_MemberForgetPassword_MemberID",
                table: "MemberForgetPassword",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyRole_PolicyID",
                table: "PolicyRole",
                column: "PolicyID");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyRole_RoleID",
                table: "PolicyRole",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_State_CountryID",
                table: "State",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_StateTranslation_StateID",
                table: "StateTranslation",
                column: "StateID");

            migrationBuilder.CreateIndex(
                name: "IX_Website_FaveIconFileID",
                table: "Website",
                column: "FaveIconFileID");

            migrationBuilder.CreateIndex(
                name: "IX_Website_LogoFileID",
                table: "Website",
                column: "LogoFileID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteIP_WebsiteID",
                table: "WebsiteIP",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteScript_WebsiteID",
                table: "WebsiteScript",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteSeoSetting_WebsiteID",
                table: "WebsiteSeoSetting",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteSocialLink_WebsiteID",
                table: "WebsiteSocialLink",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_WebstieStorageSettings_WebsiteID",
                table: "WebstieStorageSettings",
                column: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSubscribe_Member",
                table: "EmailSubscribe",
                column: "MemberID",
                principalTable: "Member",
                principalColumn: "MemberID");

            migrationBuilder.AddForeignKey(
                name: "FK_FileRecord_WebstieStorageSettings",
                table: "FileRecord",
                column: "WebsiteStorageSettingsID",
                principalTable: "WebstieStorageSettings",
                principalColumn: "WebsiteStorageSettingsID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileRecord_WebstieStorageSettings",
                table: "FileRecord");

            migrationBuilder.DropTable(
                name: "CityTranslation");

            migrationBuilder.DropTable(
                name: "ContactUsMessage");

            migrationBuilder.DropTable(
                name: "CountryTranslation");

            migrationBuilder.DropTable(
                name: "EmailSetting");

            migrationBuilder.DropTable(
                name: "EmailSubscribe");

            migrationBuilder.DropTable(
                name: "LocalizationValue");

            migrationBuilder.DropTable(
                name: "LoginTry");

            migrationBuilder.DropTable(
                name: "MemberForgetPassword");

            migrationBuilder.DropTable(
                name: "PolicyRole");

            migrationBuilder.DropTable(
                name: "StateTranslation");

            migrationBuilder.DropTable(
                name: "WebsiteIP");

            migrationBuilder.DropTable(
                name: "WebsiteScript");

            migrationBuilder.DropTable(
                name: "WebsiteSeoSetting");

            migrationBuilder.DropTable(
                name: "WebsiteSocialLink");

            migrationBuilder.DropTable(
                name: "City");

            migrationBuilder.DropTable(
                name: "LocalizationKey");

            migrationBuilder.DropTable(
                name: "Member");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "State");

            migrationBuilder.DropTable(
                name: "Language");

            migrationBuilder.DropTable(
                name: "Policy");

            migrationBuilder.DropTable(
                name: "Country");

            migrationBuilder.DropTable(
                name: "WebstieStorageSettings");

            migrationBuilder.DropTable(
                name: "Website");

            migrationBuilder.DropTable(
                name: "FileRecord");
        }
    }
}
