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
                name: "Countries",
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
                    table.PrimaryKey("PK_Countries", x => x.CountryID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    CurrencyCode = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    DecimalDigits = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.CurrencyCode);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RoleKey = table.Column<string>(type: "varchar(42)", unicode: false, maxLength: 42, nullable: false),
                    Description = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Category = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CountryTranslations",
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
                    table.PrimaryKey("PK_CountryTranslations", x => x.CountryTranslationID);
                    table.ForeignKey(
                        name: "FK_CountryTranslations_Countries",
                        column: x => x.CountryID,
                        principalTable: "Countries",
                        principalColumn: "CountryID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "States",
                columns: table => new
                {
                    StateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CountryID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "varchar(48)", maxLength: 48, nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => x.StateID);
                    table.ForeignKey(
                        name: "FK_States_Countries",
                        column: x => x.CountryID,
                        principalTable: "Countries",
                        principalColumn: "CountryID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Cities",
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
                    table.PrimaryKey("PK_Cities", x => x.CityID);
                    table.ForeignKey(
                        name: "FK_Cities_Countries",
                        column: x => x.CountryID,
                        principalTable: "Countries",
                        principalColumn: "CountryID");
                    table.ForeignKey(
                        name: "FK_Cities_States",
                        column: x => x.StateID,
                        principalTable: "States",
                        principalColumn: "StateID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StateTranslations",
                columns: table => new
                {
                    StateTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(48)", maxLength: 48, nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateTranslations", x => x.StateTranslationID);
                    table.ForeignKey(
                        name: "FK_StateTranslations_States",
                        column: x => x.StateID,
                        principalTable: "States",
                        principalColumn: "StateID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CityTranslations",
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
                    table.PrimaryKey("PK_CityTranslations", x => x.CityTranslationID);
                    table.ForeignKey(
                        name: "FK_CityTranslations_Cities",
                        column: x => x.CityID,
                        principalTable: "Cities",
                        principalColumn: "CityID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AdminNotifications",
                columns: table => new
                {
                    AdminNotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MemberID = table.Column<int>(type: "int", nullable: false),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    NotificationType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    ActionUrl = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    RelatedEntityID = table.Column<int>(type: "int", nullable: true),
                    IsRead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminNotifications", x => x.AdminNotificationID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AttributeDefinitions",
                columns: table => new
                {
                    AttributeDefinitionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    InputType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Unit = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    IsFilterable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsVariantAttribute = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsComparable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ShowOnTop = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeDefinitions", x => x.AttributeDefinitionID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AttributeDefinitionTranslations",
                columns: table => new
                {
                    AttributeDefinitionTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AttributeDefinitionID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeDefinitionTranslations", x => x.AttributeDefinitionTranslationID);
                    table.ForeignKey(
                        name: "FK_AttributeDefinitionTranslations_AttributeDefinitions",
                        column: x => x.AttributeDefinitionID,
                        principalTable: "AttributeDefinitions",
                        principalColumn: "AttributeDefinitionID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AttributeOptions",
                columns: table => new
                {
                    AttributeOptionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AttributeDefinitionID = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    ColorHex = table.Column<string>(type: "char(7)", unicode: false, fixedLength: true, maxLength: 7, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeOptions", x => x.AttributeOptionID);
                    table.ForeignKey(
                        name: "FK_AttributeOptions_AttributeDefinitions",
                        column: x => x.AttributeDefinitionID,
                        principalTable: "AttributeDefinitions",
                        principalColumn: "AttributeDefinitionID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AttributeOptionTranslations",
                columns: table => new
                {
                    AttributeOptionTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AttributeOptionID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Value = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeOptionTranslations", x => x.AttributeOptionTranslationID);
                    table.ForeignKey(
                        name: "FK_AttributeOptionTranslations_AttributeOptions",
                        column: x => x.AttributeOptionID,
                        principalTable: "AttributeOptions",
                        principalColumn: "AttributeOptionID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    BankAccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    BankID = table.Column<int>(type: "int", nullable: false),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "varchar(70)", maxLength: 70, nullable: false),
                    OwnerName = table.Column<string>(type: "varchar(90)", maxLength: 90, nullable: true),
                    AccountNumber = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    IBAN = table.Column<string>(type: "varchar(34)", unicode: false, maxLength: 34, nullable: true),
                    CardNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    IsForOfflinePayment = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedByMemberId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.BankAccountID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    BankID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    LogoFileID = table.Column<int>(type: "int", nullable: true),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BankCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banks", x => x.BankID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    BrandID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    LogoFileID = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.BrandID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BrandTranslations",
                columns: table => new
                {
                    BrandTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    BrandID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandTranslations", x => x.BrandTranslationID);
                    table.ForeignKey(
                        name: "FK_BrandTranslations_Brands",
                        column: x => x.BrandID,
                        principalTable: "Brands",
                        principalColumn: "BrandID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    CartItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CartID = table.Column<int>(type: "int", nullable: false),
                    ProductVariantID = table.Column<int>(type: "int", nullable: false),
                    VendorProductID = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.CartItemID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    CartID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: true),
                    SessionKey = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    CouponID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.CartID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    PostTypeID = table.Column<int>(type: "int", nullable: true),
                    ParentCategoryID = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryID);
                    table.ForeignKey(
                        name: "FK_Categories_Categories",
                        column: x => x.ParentCategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CategoryTranslations",
                columns: table => new
                {
                    CategoryTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryTranslations", x => x.CategoryTranslationID);
                    table.ForeignKey(
                        name: "FK_CategoryTranslations_Categories",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChartOfAccounts",
                columns: table => new
                {
                    ChartOfAccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    ParentAccountID = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    AccountType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartOfAccounts", x => x.ChartOfAccountID);
                    table.ForeignKey(
                        name: "FK_ChartOfAccounts_ChartOfAccounts",
                        column: x => x.ParentAccountID,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "ChartOfAccountID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClientBankAccounts",
                columns: table => new
                {
                    ClientBankAccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: false),
                    BankID = table.Column<int>(type: "int", nullable: true),
                    OwnerName = table.Column<string>(type: "varchar(90)", maxLength: 90, nullable: false),
                    AccountNumber = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    IBAN = table.Column<string>(type: "varchar(34)", unicode: false, maxLength: 34, nullable: true),
                    CardNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientBankAccounts", x => x.ClientBankAccountID);
                    table.ForeignKey(
                        name: "FK_ClientBankAccounts_Banks",
                        column: x => x.BankID,
                        principalTable: "Banks",
                        principalColumn: "BankID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClientWallets",
                columns: table => new
                {
                    ClientWalletID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: false),
                    BalanceUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientWallets", x => x.ClientWalletID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClientWalletTransactions",
                columns: table => new
                {
                    ClientWalletTransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    ClientWalletID = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AmountUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    BalanceAfterUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    SourceType = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    SourceId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedByMemberID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientWalletTransactions", x => x.ClientWalletTransactionID);
                    table.ForeignKey(
                        name: "FK_ClientWalletTransactions_ClientWallets",
                        column: x => x.ClientWalletID,
                        principalTable: "ClientWallets",
                        principalColumn: "ClientWalletID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClientWalletWithdrawals",
                columns: table => new
                {
                    ClientWalletWithdrawalID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: false),
                    ClientWalletID = table.Column<int>(type: "int", nullable: false),
                    ClientBankAccountID = table.Column<int>(type: "int", nullable: false),
                    AmountUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ReviewedByMemberID = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: true),
                    RejectReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    PaymentRefNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientWalletWithdrawals", x => x.ClientWalletWithdrawalID);
                    table.ForeignKey(
                        name: "FK_ClientWalletWithdrawals_ClientBankAccounts",
                        column: x => x.ClientBankAccountID,
                        principalTable: "ClientBankAccounts",
                        principalColumn: "ClientBankAccountID");
                    table.ForeignKey(
                        name: "FK_ClientWalletWithdrawals_ClientWallets",
                        column: x => x.ClientWalletID,
                        principalTable: "ClientWallets",
                        principalColumn: "ClientWalletID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ContactUsMessages",
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
                    SenderIPAddress = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    WebsiteID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactUsMessages", x => x.ContactUsMessagesID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CouponRedemptions",
                columns: table => new
                {
                    CouponRedemptionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CouponID = table.Column<int>(type: "int", nullable: false),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: false),
                    DiscountAmountUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponRedemptions", x => x.CouponRedemptionID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    CouponID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    DiscountType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MinOrderAmountUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MaxDiscountAmountUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    UsageLimitTotal = table.Column<int>(type: "int", nullable: true),
                    UsageLimitPerClient = table.Column<int>(type: "int", nullable: true),
                    TimesUsed = table.Column<int>(type: "int", nullable: false),
                    StartsAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: true),
                    EndsAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedByMemberID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.CouponID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CurrencyRates",
                columns: table => new
                {
                    CurrencyRateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    USDToCurrency = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyRates", x => x.CurrencyRateID);
                    table.ForeignKey(
                        name: "FK_CurrencyRates_Currencies",
                        column: x => x.CurrencyCode,
                        principalTable: "Currencies",
                        principalColumn: "CurrencyCode");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailAccounts",
                columns: table => new
                {
                    EmailAccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    AccountType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    EmailAddress = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Password = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    MailServer = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    SMTPPort = table.Column<int>(type: "int", nullable: false),
                    EnableSSL = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MailName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAccounts", x => x.EmailAccountID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailSubscribes",
                columns: table => new
                {
                    EmailSubscribeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    LogTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MemberID = table.Column<int>(type: "int", nullable: true),
                    Approved = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSubscribes", x => x.EmailSubscribeID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    EmailTemplateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    TemplateKey = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    Subject = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    HtmlBody = table.Column<string>(type: "longtext", nullable: false),
                    AccountType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.EmailTemplateID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailTemplateTranslations",
                columns: table => new
                {
                    EmailTemplateTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    EmailTemplateID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Subject = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    HtmlBody = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplateTranslations", x => x.EmailTemplateTranslationID);
                    table.ForeignKey(
                        name: "FK_EmailTemplateTranslations_EmailTemplates",
                        column: x => x.EmailTemplateID,
                        principalTable: "EmailTemplates",
                        principalColumn: "EmailTemplateID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FileFolders",
                columns: table => new
                {
                    FileFolderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    ParentFolderID = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileFolders", x => x.FileFolderID);
                    table.ForeignKey(
                        name: "FK_FileFolders_Parent",
                        column: x => x.ParentFolderID,
                        principalTable: "FileFolders",
                        principalColumn: "FileFolderID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FileRecords",
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
                    CDNFileCode = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    FileFolderID = table.Column<int>(type: "int", nullable: true),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    UploaderMemberID = table.Column<int>(type: "int", nullable: true),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileRecords", x => x.FileRecordID);
                    table.ForeignKey(
                        name: "FK_FileRecords_FileFolders",
                        column: x => x.FileFolderID,
                        principalTable: "FileFolders",
                        principalColumn: "FileFolderID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Websites",
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
                    IsHub = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BrandName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false, comment: "show in title of pages"),
                    LogoFileID = table.Column<int>(type: "int", nullable: true),
                    FaveIconFileID = table.Column<int>(type: "int", nullable: true),
                    DefaultCurrencyCode = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Websites", x => x.WebsiteID);
                    table.ForeignKey(
                        name: "FK_Websites_Currencies",
                        column: x => x.DefaultCurrencyCode,
                        principalTable: "Currencies",
                        principalColumn: "CurrencyCode");
                    table.ForeignKey(
                        name: "FK_Websites_FileRecord1",
                        column: x => x.FaveIconFileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_Websites_FileRecords",
                        column: x => x.LogoFileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FileTags",
                columns: table => new
                {
                    FileTagID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileTags", x => x.FileTagID);
                    table.ForeignKey(
                        name: "FK_FileTags_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Forms",
                columns: table => new
                {
                    FormID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    FormType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SubmitButtonText = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    SuccessMessage = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    RequireLogin = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowMultipleSubmissions = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ShowResults = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NotifyEmail = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    StartAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    EndAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forms", x => x.FormID);
                    table.ForeignKey(
                        name: "FK_Forms_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    JournalEntryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    EntryNumber = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    EntryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    SourceType = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    SourceId = table.Column<int>(type: "int", nullable: true),
                    IsPosted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.JournalEntryID);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    LanguageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    LanguageCodeISO = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RTLDesign = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WebsiteID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.LanguageID);
                    table.ForeignKey(
                        name: "FK_Languages_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LocalizationKeys",
                columns: table => new
                {
                    LocalizationKeyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ItemKey = table.Column<string>(type: "varchar(72)", unicode: false, maxLength: 72, nullable: false),
                    DefaultValue = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    WebsiteID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationKeys", x => x.LocalizationKeyID);
                    table.ForeignKey(
                        name: "FK_LocalizationKeys_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LoginTries",
                columns: table => new
                {
                    LoginTryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    LogTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsSuccess = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TryIP = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    WebsiteID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginTries", x => x.LoginTryID);
                    table.ForeignKey(
                        name: "FK_LoginTries_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MediaSets",
                columns: table => new
                {
                    MediaSetID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    IsShared = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaSets", x => x.MediaSetID);
                    table.ForeignKey(
                        name: "FK_MediaSets_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    MenuID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.MenuID);
                    table.ForeignKey(
                        name: "FK_Menus_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PaymentGateways",
                columns: table => new
                {
                    PaymentGatewayID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Provider = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    MerchantID = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    ApiKey = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    ApiSecret = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    IsSandbox = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentGateways", x => x.PaymentGatewayID);
                    table.ForeignKey(
                        name: "FK_PaymentGateways_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    PolicyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WebsiteID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.PolicyID);
                    table.ForeignKey(
                        name: "FK_Policies_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PostTypes",
                columns: table => new
                {
                    PostTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    HasCategories = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HasTags = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HasAuthor = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CommentsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostTypes", x => x.PostTypeID);
                    table.ForeignKey(
                        name: "FK_PostTypes_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    ProductCategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    ParentCategoryID = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    ImageFileID = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.ProductCategoryID);
                    table.ForeignKey(
                        name: "FK_ProductCategories_FileRecords",
                        column: x => x.ImageFileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_ProductCategories_ProductCategories",
                        column: x => x.ParentCategoryID,
                        principalTable: "ProductCategories",
                        principalColumn: "ProductCategoryID");
                    table.ForeignKey(
                        name: "FK_ProductCategories_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ShippingMethods",
                columns: table => new
                {
                    ShippingMethodID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    CarrierName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingMethods", x => x.ShippingMethodID);
                    table.ForeignKey(
                        name: "FK_ShippingMethods_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Slideshows",
                columns: table => new
                {
                    SlideshowID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    PlacementKey = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    TransitionEffect = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AutoPlay = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IntervalMs = table.Column<int>(type: "int", nullable: false),
                    ShowArrows = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ShowDots = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EnableLightbox = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AspectRatio = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slideshows", x => x.SlideshowID);
                    table.ForeignKey(
                        name: "FK_Slideshows_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    SupplierID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.SupplierID);
                    table.ForeignKey(
                        name: "FK_Suppliers_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    TagID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.TagID);
                    table.ForeignKey(
                        name: "FK_Tags_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TaxRates",
                columns: table => new
                {
                    TaxRateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    CountryID = table.Column<int>(type: "int", nullable: true),
                    StateID = table.Column<int>(type: "int", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRates", x => x.TaxRateID);
                    table.ForeignKey(
                        name: "FK_TaxRates_Countries",
                        column: x => x.CountryID,
                        principalTable: "Countries",
                        principalColumn: "CountryID");
                    table.ForeignKey(
                        name: "FK_TaxRates_States",
                        column: x => x.StateID,
                        principalTable: "States",
                        principalColumn: "StateID");
                    table.ForeignKey(
                        name: "FK_TaxRates_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Warranties",
                columns: table => new
                {
                    WarrantyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    ProviderName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    DurationMonths = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warranties", x => x.WarrantyID);
                    table.ForeignKey(
                        name: "FK_Warranties_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteCaptchaSettings",
                columns: table => new
                {
                    WebsiteCaptchaSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    TurnstileSiteKey = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    TurnstileSecretKey = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteCaptchaSettings", x => x.WebsiteCaptchaSettingID);
                    table.ForeignKey(
                        name: "FK_WebsiteCaptchaSettings_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteClients",
                columns: table => new
                {
                    WebsiteClientID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    AvatarID = table.Column<int>(type: "int", nullable: true),
                    Email = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: true),
                    Cellphone = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    CountryCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: true),
                    Password = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RegisterDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    Givenname = table.Column<string>(type: "varchar(42)", maxLength: 42, nullable: true),
                    Surname = table.Column<string>(type: "varchar(42)", maxLength: 42, nullable: true),
                    HashKey = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClientLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteClients", x => x.WebsiteClientID);
                    table.ForeignKey(
                        name: "FK_WebsiteClients_FileRecords",
                        column: x => x.AvatarID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_WebsiteClients_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteFeatures",
                columns: table => new
                {
                    WebsiteFeatureID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    FeatureKey = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteFeatures", x => x.WebsiteFeatureID);
                    table.ForeignKey(
                        name: "FK_WebsiteFeatures_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteIPs",
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
                    table.PrimaryKey("PK_WebsiteIPs", x => x.WebsiteIPID);
                    table.ForeignKey(
                        name: "FK_WebsiteIPs_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteRedirects",
                columns: table => new
                {
                    WebsiteRedirectID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    SourcePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    TargetPath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    IsRegex = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HitCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteRedirects", x => x.WebsiteRedirectID);
                    table.ForeignKey(
                        name: "FK_WebsiteRedirects_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteScripts",
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
                    table.PrimaryKey("PK_WebsiteScripts", x => x.WebsiteScriptID);
                    table.ForeignKey(
                        name: "FK_WebsiteScripts_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteSeoSettings",
                columns: table => new
                {
                    WebsiteSeoSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    DefaultMetaTitle = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true, comment: "Page | {SiteName}"),
                    TitleSeparator = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: true),
                    DefaultMetaDescription = table.Column<string>(type: "varchar(158)", maxLength: 158, nullable: true),
                    SitemapEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RobotsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CustomRobotsTxt = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteSeoSettings", x => x.WebsiteSeoSettingID);
                    table.ForeignKey(
                        name: "FK_WebsiteSeoSettings_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteSocialLinks",
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
                    table.PrimaryKey("PK_WebsiteSocialLinks", x => x.WebsiteSocialLinkID);
                    table.ForeignKey(
                        name: "FK_WebsiteSocialLinks_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteStorageSettings",
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
                    table.PrimaryKey("PK_WebsiteStorageSettings", x => x.WebsiteStorageSettingsID);
                    table.ForeignKey(
                        name: "FK_WebsiteStorageSettings_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteThemes",
                columns: table => new
                {
                    WebsiteThemeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Slug = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    Author = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    HasScreenshot = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteThemes", x => x.WebsiteThemeID);
                    table.ForeignKey(
                        name: "FK_WebsiteThemes_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteWatermarkSettings",
                columns: table => new
                {
                    WebsiteWatermarkSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    WatermarkFileID = table.Column<int>(type: "int", nullable: true),
                    Position = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SizePercent = table.Column<int>(type: "int", nullable: false),
                    Opacity = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteWatermarkSettings", x => x.WebsiteWatermarkSettingID);
                    table.ForeignKey(
                        name: "FK_WebsiteWatermarkSettings_FileRecords",
                        column: x => x.WatermarkFileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_WebsiteWatermarkSettings_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FileRecordTags",
                columns: table => new
                {
                    FileRecordTagID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FileRecordID = table.Column<int>(type: "int", nullable: false),
                    FileTagID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileRecordTags", x => x.FileRecordTagID);
                    table.ForeignKey(
                        name: "FK_FileRecordTags_FileRecords",
                        column: x => x.FileRecordID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_FileRecordTags_FileTags",
                        column: x => x.FileTagID,
                        principalTable: "FileTags",
                        principalColumn: "FileTagID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FormFields",
                columns: table => new
                {
                    FormFieldID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FormID = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    FieldType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Placeholder = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    HelpText = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    IsRequired = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MinValue = table.Column<int>(type: "int", nullable: true),
                    MaxValue = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormFields", x => x.FormFieldID);
                    table.ForeignKey(
                        name: "FK_FormFields_Forms",
                        column: x => x.FormID,
                        principalTable: "Forms",
                        principalColumn: "FormID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JournalEntryLines",
                columns: table => new
                {
                    JournalEntryLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    JournalEntryID = table.Column<int>(type: "int", nullable: false),
                    ChartOfAccountID = table.Column<int>(type: "int", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Description = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryLines", x => x.JournalEntryLineID);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_ChartOfAccounts",
                        column: x => x.ChartOfAccountID,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "ChartOfAccountID");
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_JournalEntries",
                        column: x => x.JournalEntryID,
                        principalTable: "JournalEntries",
                        principalColumn: "JournalEntryID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LocalizationValues",
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
                    table.PrimaryKey("PK_LocalizationValues", x => x.LocalizationValueID);
                    table.ForeignKey(
                        name: "FK_LocalizationValues_LocalizationKeys",
                        column: x => x.LocalizationKeyID,
                        principalTable: "LocalizationKeys",
                        principalColumn: "LocalizationKeyID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MediaSetItems",
                columns: table => new
                {
                    MediaSetItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MediaSetID = table.Column<int>(type: "int", nullable: false),
                    FileID = table.Column<int>(type: "int", nullable: true),
                    VideoThumbnailFileID = table.Column<int>(type: "int", nullable: true),
                    ExternalVideoUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaSetItems", x => x.MediaSetItemID);
                    table.ForeignKey(
                        name: "FK_MediaSetItems_FileRecord1",
                        column: x => x.VideoThumbnailFileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_MediaSetItems_FileRecords",
                        column: x => x.FileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_MediaSetItems_MediaSets",
                        column: x => x.MediaSetID,
                        principalTable: "MediaSets",
                        principalColumn: "MediaSetID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Members",
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
                    AvatarID = table.Column<int>(type: "int", nullable: true),
                    HashKey = table.Column<Guid>(type: "char(36)", nullable: false),
                    PolicyID = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    AdminUIMode = table.Column<byte>(type: "tinyint unsigned", nullable: false, comment: "0 = Basic (admin surface reduced to what this member's Website.WebsiteType needs), 1 = General (same admin surface regardless of website type), 2 = Advanced (full surface, e.g. extra-language pages/buttons on an otherwise single-language site)"),
                    IsSiteAdmin = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.MemberID);
                    table.ForeignKey(
                        name: "FK_Members_FileRecords",
                        column: x => x.AvatarID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_Members_Policies",
                        column: x => x.PolicyID,
                        principalTable: "Policies",
                        principalColumn: "PolicyID");
                    table.ForeignKey(
                        name: "FK_Members_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PolicyRoles",
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
                    table.PrimaryKey("PK_PolicyRoles", x => x.PolicyRoleID);
                    table.ForeignKey(
                        name: "FK_PolicyRoles_Policies",
                        column: x => x.PolicyID,
                        principalTable: "Policies",
                        principalColumn: "PolicyID");
                    table.ForeignKey(
                        name: "FK_PolicyRoles_Roles",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductCategoryTranslations",
                columns: table => new
                {
                    ProductCategoryTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductCategoryID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategoryTranslations", x => x.ProductCategoryTranslationID);
                    table.ForeignKey(
                        name: "FK_ProductCategoryTranslations_ProductCategories",
                        column: x => x.ProductCategoryID,
                        principalTable: "ProductCategories",
                        principalColumn: "ProductCategoryID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ShippingRates",
                columns: table => new
                {
                    ShippingRateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ShippingMethodID = table.Column<int>(type: "int", nullable: false),
                    CountryID = table.Column<int>(type: "int", nullable: true),
                    StateID = table.Column<int>(type: "int", nullable: true),
                    CityID = table.Column<int>(type: "int", nullable: true),
                    MinWeightKg = table.Column<decimal>(type: "decimal(10,3)", nullable: true),
                    MaxWeightKg = table.Column<decimal>(type: "decimal(10,3)", nullable: true),
                    PriceUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingRates", x => x.ShippingRateID);
                    table.ForeignKey(
                        name: "FK_ShippingRates_Cities",
                        column: x => x.CityID,
                        principalTable: "Cities",
                        principalColumn: "CityID");
                    table.ForeignKey(
                        name: "FK_ShippingRates_Countries",
                        column: x => x.CountryID,
                        principalTable: "Countries",
                        principalColumn: "CountryID");
                    table.ForeignKey(
                        name: "FK_ShippingRates_ShippingMethods",
                        column: x => x.ShippingMethodID,
                        principalTable: "ShippingMethods",
                        principalColumn: "ShippingMethodID");
                    table.ForeignKey(
                        name: "FK_ShippingRates_States",
                        column: x => x.StateID,
                        principalTable: "States",
                        principalColumn: "StateID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SlideshowSlides",
                columns: table => new
                {
                    SlideshowSlideID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SlideshowID = table.Column<int>(type: "int", nullable: false),
                    FileID = table.Column<int>(type: "int", nullable: false),
                    MobileFileID = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Caption = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    ButtonText = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    LinkUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    OpenInNewTab = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EndAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlideshowSlides", x => x.SlideshowSlideID);
                    table.ForeignKey(
                        name: "FK_SlideshowSlides_FileRecord1",
                        column: x => x.MobileFileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_SlideshowSlides_FileRecords",
                        column: x => x.FileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_SlideshowSlides_Slideshows",
                        column: x => x.SlideshowID,
                        principalTable: "Slideshows",
                        principalColumn: "SlideshowID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TagTranslations",
                columns: table => new
                {
                    TagTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    TagID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagTranslations", x => x.TagTranslationID);
                    table.ForeignKey(
                        name: "FK_TagTranslations_Tags",
                        column: x => x.TagID,
                        principalTable: "Tags",
                        principalColumn: "TagID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WarrantyTranslations",
                columns: table => new
                {
                    WarrantyTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WarrantyID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    ProviderName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyTranslations", x => x.WarrantyTranslationID);
                    table.ForeignKey(
                        name: "FK_WarrantyTranslations_Warranties",
                        column: x => x.WarrantyID,
                        principalTable: "Warranties",
                        principalColumn: "WarrantyID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FormResponses",
                columns: table => new
                {
                    FormResponseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FormID = table.Column<int>(type: "int", nullable: false),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: true),
                    SenderIPAddress = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormResponses", x => x.FormResponseID);
                    table.ForeignKey(
                        name: "FK_FormResponses_Forms",
                        column: x => x.FormID,
                        principalTable: "Forms",
                        principalColumn: "FormID");
                    table.ForeignKey(
                        name: "FK_FormResponses_WebsiteClients",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteClientAddresses",
                columns: table => new
                {
                    WebsiteClientAddressID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ReceiverName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    AddressLine = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    PostalCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteClientAddresses", x => x.WebsiteClientAddressID);
                    table.ForeignKey(
                        name: "FK_WebsiteClientAddresses_Cities",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "CityID");
                    table.ForeignKey(
                        name: "FK_WebsiteClientAddresses_Countries",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryID");
                    table.ForeignKey(
                        name: "FK_WebsiteClientAddresses_WebsiteClients",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebsiteClientForgetPasswords",
                columns: table => new
                {
                    WebsiteClientForgetPasswordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ForgetKey = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: false),
                    LogTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteClientForgetPasswords", x => x.WebsiteClientForgetPasswordID);
                    table.ForeignKey(
                        name: "FK_WebsiteClientForgetPasswords_WebsiteClients",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Wishlists",
                columns: table => new
                {
                    WishlistID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wishlists", x => x.WishlistID);
                    table.ForeignKey(
                        name: "FK_Wishlists_WebsiteClients",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID");
                    table.ForeignKey(
                        name: "FK_Wishlists_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FormFieldOptions",
                columns: table => new
                {
                    FormFieldOptionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FormFieldID = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Value = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormFieldOptions", x => x.FormFieldOptionID);
                    table.ForeignKey(
                        name: "FK_FormFieldOptions_FormFields",
                        column: x => x.FormFieldID,
                        principalTable: "FormFields",
                        principalColumn: "FormFieldID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MemberForgetPasswords",
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
                    table.PrimaryKey("PK_MemberForgetPasswords", x => x.MemberForgetPasswordID);
                    table.ForeignKey(
                        name: "FK_MemberForgetPasswords_Members",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    PageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    ParentPageID = table.Column<int>(type: "int", nullable: true),
                    Slug = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Title = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Content = table.Column<string>(type: "longtext", nullable: true),
                    Template = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    IsHomepage = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedByMemberID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.PageID);
                    table.ForeignKey(
                        name: "FK_Pages_Members",
                        column: x => x.CreatedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_Pages_Pages",
                        column: x => x.ParentPageID,
                        principalTable: "Pages",
                        principalColumn: "PageID");
                    table.ForeignKey(
                        name: "FK_Pages_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    PostID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    PostTypeID = table.Column<int>(type: "int", nullable: false),
                    AuthorMemberID = table.Column<int>(type: "int", nullable: true),
                    Slug = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Title = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Excerpt = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    Content = table.Column<string>(type: "longtext", nullable: true),
                    FeaturedImageFileID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsFeatured = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    CommentsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.PostID);
                    table.ForeignKey(
                        name: "FK_Posts_FileRecords",
                        column: x => x.FeaturedImageFileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_Posts_Members",
                        column: x => x.AuthorMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_Posts_PostTypes",
                        column: x => x.PostTypeID,
                        principalTable: "PostTypes",
                        principalColumn: "PostTypeID");
                    table.ForeignKey(
                        name: "FK_Posts_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    BrandID = table.Column<int>(type: "int", nullable: true),
                    Slug = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Title = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    ShortDescription = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    Content = table.Column<string>(type: "longtext", nullable: true),
                    ExpertReview = table.Column<string>(type: "longtext", nullable: true),
                    FeaturedImageFileID = table.Column<int>(type: "int", nullable: true),
                    IsCatalogOnly = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HasVariants = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AvgRating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    RatingCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedByMemberID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                    table.ForeignKey(
                        name: "FK_Products_Brands",
                        column: x => x.BrandID,
                        principalTable: "Brands",
                        principalColumn: "BrandID");
                    table.ForeignKey(
                        name: "FK_Products_FileRecords",
                        column: x => x.FeaturedImageFileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_Products_Members",
                        column: x => x.CreatedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_Products_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    VendorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    LogoFileID = table.Column<int>(type: "int", nullable: true),
                    Rating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SettlementMode = table.Column<byte>(type: "tinyint unsigned", nullable: false, comment: "0 = Immediate: every purchase from this vendor is settled instantly like a normal cash purchase. 1 = Credit: purchases accrue as credit and are batched into a periodic Settlements record due on CreditDays"),
                    CreditDays = table.Column<int>(type: "int", nullable: true, comment: "number of days after the settlement period ends before payment is due; only meaningful when SettlementMode = Credit"),
                    CreditLimitUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: true, comment: "maximum outstanding credit balance allowed for this vendor, in USD; only meaningful when SettlementMode = Credit"),
                    VendorType = table.Column<byte>(type: "tinyint unsigned", nullable: false, comment: "0 = Display-only title, 1 = Member login (manage own catalog), 2 = Linked website (inter-site virtual credit)"),
                    MemberID = table.Column<int>(type: "int", nullable: true),
                    LinkedWebsiteID = table.Column<int>(type: "int", nullable: true),
                    AvailableCreditUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.VendorID);
                    table.ForeignKey(
                        name: "FK_Vendors_FileRecords",
                        column: x => x.LogoFileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_Vendors_LinkedWebsites",
                        column: x => x.LinkedWebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                    table.ForeignKey(
                        name: "FK_Vendors_Members",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_Vendors_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FormResponseValues",
                columns: table => new
                {
                    FormResponseValueID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FormResponseID = table.Column<int>(type: "int", nullable: false),
                    FormFieldID = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormResponseValues", x => x.FormResponseValueID);
                    table.ForeignKey(
                        name: "FK_FormResponseValues_FormFields",
                        column: x => x.FormFieldID,
                        principalTable: "FormFields",
                        principalColumn: "FormFieldID");
                    table.ForeignKey(
                        name: "FK_FormResponseValues_FormResponses",
                        column: x => x.FormResponseID,
                        principalTable: "FormResponses",
                        principalColumn: "FormResponseID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    OrderNumber = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CurrencyCode = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    ExchangeRateToUsd = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ShippingTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    GrandTotalUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    WebsiteClientAddressID = table.Column<int>(type: "int", nullable: true),
                    AddressSnapshot = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    CouponID = table.Column<int>(type: "int", nullable: true),
                    ShippingMethodID = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    CreatedByMemberID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderID);
                    table.ForeignKey(
                        name: "FK_Orders_Coupons",
                        column: x => x.CouponID,
                        principalTable: "Coupons",
                        principalColumn: "CouponID");
                    table.ForeignKey(
                        name: "FK_Orders_Currencies",
                        column: x => x.CurrencyCode,
                        principalTable: "Currencies",
                        principalColumn: "CurrencyCode");
                    table.ForeignKey(
                        name: "FK_Orders_Members",
                        column: x => x.CreatedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_Orders_ShippingMethods",
                        column: x => x.ShippingMethodID,
                        principalTable: "ShippingMethods",
                        principalColumn: "ShippingMethodID");
                    table.ForeignKey(
                        name: "FK_Orders_WebsiteClientAddresses",
                        column: x => x.WebsiteClientAddressID,
                        principalTable: "WebsiteClientAddresses",
                        principalColumn: "WebsiteClientAddressID");
                    table.ForeignKey(
                        name: "FK_Orders_WebsiteClients",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID");
                    table.ForeignKey(
                        name: "FK_Orders_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PageTranslations",
                columns: table => new
                {
                    PageTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PageID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Slug = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Content = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageTranslations", x => x.PageTranslationID);
                    table.ForeignKey(
                        name: "FK_PageTranslations_Pages",
                        column: x => x.PageID,
                        principalTable: "Pages",
                        principalColumn: "PageID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PostCategories",
                columns: table => new
                {
                    PostID = table.Column<int>(type: "int", nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostCategories", x => new { x.PostID, x.CategoryID });
                    table.ForeignKey(
                        name: "FK_PostCategories_Categories",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID");
                    table.ForeignKey(
                        name: "FK_PostCategories_Posts",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "PostID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PostTags",
                columns: table => new
                {
                    PostID = table.Column<int>(type: "int", nullable: false),
                    TagID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostTags", x => new { x.PostID, x.TagID });
                    table.ForeignKey(
                        name: "FK_PostTags_Posts",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "PostID");
                    table.ForeignKey(
                        name: "FK_PostTags_Tags",
                        column: x => x.TagID,
                        principalTable: "Tags",
                        principalColumn: "TagID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PostTranslations",
                columns: table => new
                {
                    PostTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PostID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Slug = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Excerpt = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    Content = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostTranslations", x => x.PostTranslationID);
                    table.ForeignKey(
                        name: "FK_PostTranslations_Posts",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "PostID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductAttributeValues",
                columns: table => new
                {
                    ProductAttributeValueID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    AttributeDefinitionID = table.Column<int>(type: "int", nullable: false),
                    AttributeOptionID = table.Column<int>(type: "int", nullable: true),
                    CustomValue = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    NumericValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IsFeatured = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributeValues", x => x.ProductAttributeValueID);
                    table.ForeignKey(
                        name: "FK_ProductAttributeValues_AttributeDefinitions",
                        column: x => x.AttributeDefinitionID,
                        principalTable: "AttributeDefinitions",
                        principalColumn: "AttributeDefinitionID");
                    table.ForeignKey(
                        name: "FK_ProductAttributeValues_AttributeOptions",
                        column: x => x.AttributeOptionID,
                        principalTable: "AttributeOptions",
                        principalColumn: "AttributeOptionID");
                    table.ForeignKey(
                        name: "FK_ProductAttributeValues_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductCategoryMaps",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    ProductCategoryID = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategoryMaps", x => new { x.ProductID, x.ProductCategoryID });
                    table.ForeignKey(
                        name: "FK_ProductCategoryMaps_ProductCategories",
                        column: x => x.ProductCategoryID,
                        principalTable: "ProductCategories",
                        principalColumn: "ProductCategoryID");
                    table.ForeignKey(
                        name: "FK_ProductCategoryMaps_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductCategoryRelations",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    RelatedProductCategoryID = table.Column<int>(type: "int", nullable: false),
                    RelationType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    MaxItems = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategoryRelations", x => new { x.ProductID, x.RelatedProductCategoryID, x.RelationType });
                    table.ForeignKey(
                        name: "FK_ProductCategoryRelations_ProductCategories",
                        column: x => x.RelatedProductCategoryID,
                        principalTable: "ProductCategories",
                        principalColumn: "ProductCategoryID");
                    table.ForeignKey(
                        name: "FK_ProductCategoryRelations_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductMedia",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    MediaSetID = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMedia", x => new { x.ProductID, x.MediaSetID });
                    table.ForeignKey(
                        name: "FK_ProductMedia_MediaSets",
                        column: x => x.MediaSetID,
                        principalTable: "MediaSets",
                        principalColumn: "MediaSetID");
                    table.ForeignKey(
                        name: "FK_ProductMedia_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductQuestions",
                columns: table => new
                {
                    ProductQuestionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: false),
                    Body = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    Approved = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductQuestions", x => x.ProductQuestionID);
                    table.ForeignKey(
                        name: "FK_ProductQuestions_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                    table.ForeignKey(
                        name: "FK_ProductQuestions_WebsiteClients",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID");
                    table.ForeignKey(
                        name: "FK_ProductQuestions_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductRelations",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    RelatedProductID = table.Column<int>(type: "int", nullable: false),
                    RelationType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRelations", x => new { x.ProductID, x.RelatedProductID, x.RelationType });
                    table.ForeignKey(
                        name: "FK_ProductRelations_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                    table.ForeignKey(
                        name: "FK_ProductRelations_Products1",
                        column: x => x.RelatedProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductTranslations",
                columns: table => new
                {
                    ProductTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Slug = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    ShortDescription = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    Content = table.Column<string>(type: "longtext", nullable: true),
                    ExpertReview = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTranslations", x => x.ProductTranslationID);
                    table.ForeignKey(
                        name: "FK_ProductTranslations_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    ProductVariantID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    Sku = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ImageFileID = table.Column<int>(type: "int", nullable: true),
                    ReferencePriceUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CompareAtPriceUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    OverridePrice = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(10,3)", nullable: true),
                    Barcode = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.ProductVariantID);
                    table.ForeignKey(
                        name: "FK_ProductVariants_FileRecords",
                        column: x => x.ImageFileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                    table.ForeignKey(
                        name: "FK_ProductVariants_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductWarnings",
                columns: table => new
                {
                    ProductWarningID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Text = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductWarnings", x => x.ProductWarningID);
                    table.ForeignKey(
                        name: "FK_ProductWarnings_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductWarranties",
                columns: table => new
                {
                    ProductWarrantyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    WarrantyID = table.Column<int>(type: "int", nullable: true),
                    CustomTitle = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    CustomDescription = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductWarranties", x => x.ProductWarrantyID);
                    table.ForeignKey(
                        name: "FK_ProductWarranties_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                    table.ForeignKey(
                        name: "FK_ProductWarranties_Warranties",
                        column: x => x.WarrantyID,
                        principalTable: "Warranties",
                        principalColumn: "WarrantyID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    MenuItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MenuID = table.Column<int>(type: "int", nullable: false),
                    ParentItemID = table.Column<int>(type: "int", nullable: true),
                    ItemType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PageID = table.Column<int>(type: "int", nullable: true),
                    PostID = table.Column<int>(type: "int", nullable: true),
                    CategoryID = table.Column<int>(type: "int", nullable: true),
                    ProductID = table.Column<int>(type: "int", nullable: true),
                    ProductCategoryID = table.Column<int>(type: "int", nullable: true),
                    BrandID = table.Column<int>(type: "int", nullable: true),
                    VendorID = table.Column<int>(type: "int", nullable: true),
                    Url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Icon = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    CssClass = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    OpenInNewTab = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.MenuItemID);
                    table.ForeignKey(
                        name: "FK_MenuItems_Brands",
                        column: x => x.BrandID,
                        principalTable: "Brands",
                        principalColumn: "BrandID");
                    table.ForeignKey(
                        name: "FK_MenuItems_Categories",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID");
                    table.ForeignKey(
                        name: "FK_MenuItems_MenuItems",
                        column: x => x.ParentItemID,
                        principalTable: "MenuItems",
                        principalColumn: "MenuItemID");
                    table.ForeignKey(
                        name: "FK_MenuItems_Menus",
                        column: x => x.MenuID,
                        principalTable: "Menus",
                        principalColumn: "MenuID");
                    table.ForeignKey(
                        name: "FK_MenuItems_Pages",
                        column: x => x.PageID,
                        principalTable: "Pages",
                        principalColumn: "PageID");
                    table.ForeignKey(
                        name: "FK_MenuItems_Posts",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "PostID");
                    table.ForeignKey(
                        name: "FK_MenuItems_ProductCategories",
                        column: x => x.ProductCategoryID,
                        principalTable: "ProductCategories",
                        principalColumn: "ProductCategoryID");
                    table.ForeignKey(
                        name: "FK_MenuItems_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                    table.ForeignKey(
                        name: "FK_MenuItems_Vendors",
                        column: x => x.VendorID,
                        principalTable: "Vendors",
                        principalColumn: "VendorID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Settlements",
                columns: table => new
                {
                    SettlementID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    TargetType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    VendorID = table.Column<int>(type: "int", nullable: true),
                    TargetWebsiteID = table.Column<int>(type: "int", nullable: true),
                    SupplierID = table.Column<int>(type: "int", nullable: true),
                    PeriodFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodTo = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    BankAccountID = table.Column<int>(type: "int", nullable: true),
                    PaymentRefNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedByMemberID = table.Column<int>(type: "int", nullable: true),
                    ApprovedByMemberID = table.Column<int>(type: "int", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settlements", x => x.SettlementID);
                    table.ForeignKey(
                        name: "FK_Settlements_BankAccounts",
                        column: x => x.BankAccountID,
                        principalTable: "BankAccounts",
                        principalColumn: "BankAccountID");
                    table.ForeignKey(
                        name: "FK_Settlements_Currencies",
                        column: x => x.CurrencyCode,
                        principalTable: "Currencies",
                        principalColumn: "CurrencyCode");
                    table.ForeignKey(
                        name: "FK_Settlements_Member1",
                        column: x => x.ApprovedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_Settlements_Members",
                        column: x => x.CreatedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_Settlements_Suppliers",
                        column: x => x.SupplierID,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierID");
                    table.ForeignKey(
                        name: "FK_Settlements_Vendors",
                        column: x => x.VendorID,
                        principalTable: "Vendors",
                        principalColumn: "VendorID");
                    table.ForeignKey(
                        name: "FK_Settlements_Website1",
                        column: x => x.TargetWebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                    table.ForeignKey(
                        name: "FK_Settlements_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VendorTranslations",
                columns: table => new
                {
                    VendorTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    VendorID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorTranslations", x => x.VendorTranslationID);
                    table.ForeignKey(
                        name: "FK_VendorTranslations_Vendors",
                        column: x => x.VendorID,
                        principalTable: "Vendors",
                        principalColumn: "VendorID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderStatusHistories",
                columns: table => new
                {
                    OrderStatusHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    ToStatus = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedByMemberID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistories", x => x.OrderStatusHistoryID);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistories_Members",
                        column: x => x.CreatedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_OrderStatusHistories_Orders",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    OrderID = table.Column<int>(type: "int", nullable: true),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: false),
                    Method = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PaymentGatewayID = table.Column<int>(type: "int", nullable: true),
                    BankAccountID = table.Column<int>(type: "int", nullable: true),
                    ClientWalletTransactionID = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    ExchangeRateToUsd = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    AmountUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    GatewayRefNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    TrackingCode = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ReceiptFileID = table.Column<int>(type: "int", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: true),
                    VerifiedByMemberID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentID);
                    table.ForeignKey(
                        name: "FK_Payments_BankAccounts",
                        column: x => x.BankAccountID,
                        principalTable: "BankAccounts",
                        principalColumn: "BankAccountID");
                    table.ForeignKey(
                        name: "FK_Payments_ClientWalletTransactions",
                        column: x => x.ClientWalletTransactionID,
                        principalTable: "ClientWalletTransactions",
                        principalColumn: "ClientWalletTransactionID");
                    table.ForeignKey(
                        name: "FK_Payments_Currencies",
                        column: x => x.CurrencyCode,
                        principalTable: "Currencies",
                        principalColumn: "CurrencyCode");
                    table.ForeignKey(
                        name: "FK_Payments_FileRecords",
                        column: x => x.ReceiptFileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_Payments_Members",
                        column: x => x.VerifiedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_Payments_Orders",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
                    table.ForeignKey(
                        name: "FK_Payments_PaymentGateways",
                        column: x => x.PaymentGatewayID,
                        principalTable: "PaymentGateways",
                        principalColumn: "PaymentGatewayID");
                    table.ForeignKey(
                        name: "FK_Payments_WebsiteClients",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID");
                    table.ForeignKey(
                        name: "FK_Payments_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductAttributeValueTranslations",
                columns: table => new
                {
                    ProductAttributeValueTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductAttributeValueID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    CustomValue = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributeValueTranslations", x => x.ProductAttributeValueTranslationID);
                    table.ForeignKey(
                        name: "FK_ProductAttributeValueTranslations_ProductAttributeValues",
                        column: x => x.ProductAttributeValueID,
                        principalTable: "ProductAttributeValues",
                        principalColumn: "ProductAttributeValueID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductAnswers",
                columns: table => new
                {
                    ProductAnswerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductQuestionID = table.Column<int>(type: "int", nullable: false),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: true),
                    VendorID = table.Column<int>(type: "int", nullable: true),
                    Body = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    LikeCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    Approved = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAnswers", x => x.ProductAnswerID);
                    table.ForeignKey(
                        name: "FK_ProductAnswers_ProductQuestions",
                        column: x => x.ProductQuestionID,
                        principalTable: "ProductQuestions",
                        principalColumn: "ProductQuestionID");
                    table.ForeignKey(
                        name: "FK_ProductAnswers_Vendors",
                        column: x => x.VendorID,
                        principalTable: "Vendors",
                        principalColumn: "VendorID");
                    table.ForeignKey(
                        name: "FK_ProductAnswers_WebsiteClients",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    InventoryItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    ProductVariantID = table.Column<int>(type: "int", nullable: false),
                    QuantityOnHand = table.Column<int>(type: "int", nullable: false),
                    QuantityReserved = table.Column<int>(type: "int", nullable: false),
                    ReorderLevel = table.Column<int>(type: "int", nullable: false),
                    AvgCostUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.InventoryItemID);
                    table.ForeignKey(
                        name: "FK_InventoryItems_ProductVariants",
                        column: x => x.ProductVariantID,
                        principalTable: "ProductVariants",
                        principalColumn: "ProductVariantID");
                    table.ForeignKey(
                        name: "FK_InventoryItems_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductReviews",
                columns: table => new
                {
                    ProductReviewID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    ProductVariantID = table.Column<int>(type: "int", nullable: true),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Body = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false),
                    ProsJson = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    ConsJson = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    IsVerifiedPurchase = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    LikeCount = table.Column<int>(type: "int", nullable: false),
                    DislikeCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    Approved = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReviews", x => x.ProductReviewID);
                    table.ForeignKey(
                        name: "FK_ProductReviews_ProductVariants",
                        column: x => x.ProductVariantID,
                        principalTable: "ProductVariants",
                        principalColumn: "ProductVariantID");
                    table.ForeignKey(
                        name: "FK_ProductReviews_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                    table.ForeignKey(
                        name: "FK_ProductReviews_WebsiteClients",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID");
                    table.ForeignKey(
                        name: "FK_ProductReviews_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductVariantPriceHistories",
                columns: table => new
                {
                    ProductVariantPriceHistoryID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductVariantID = table.Column<int>(type: "int", nullable: false),
                    ReferencePriceUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CompareAtPriceUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    ChangedByMemberId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariantPriceHistories", x => x.ProductVariantPriceHistoryID);
                    table.ForeignKey(
                        name: "FK_ProductVariantPriceHistories_Members",
                        column: x => x.ChangedByMemberId,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_ProductVariantPriceHistories_ProductVariants",
                        column: x => x.ProductVariantID,
                        principalTable: "ProductVariants",
                        principalColumn: "ProductVariantID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VariantAttributeValues",
                columns: table => new
                {
                    ProductVariantID = table.Column<int>(type: "int", nullable: false),
                    AttributeDefinitionID = table.Column<int>(type: "int", nullable: false),
                    AttributeOptionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantAttributeValues", x => new { x.ProductVariantID, x.AttributeDefinitionID });
                    table.ForeignKey(
                        name: "FK_VariantAttributeValues_AttributeDefinitions",
                        column: x => x.AttributeDefinitionID,
                        principalTable: "AttributeDefinitions",
                        principalColumn: "AttributeDefinitionID");
                    table.ForeignKey(
                        name: "FK_VariantAttributeValues_AttributeOptions",
                        column: x => x.AttributeOptionID,
                        principalTable: "AttributeOptions",
                        principalColumn: "AttributeOptionID");
                    table.ForeignKey(
                        name: "FK_VariantAttributeValues_ProductVariants",
                        column: x => x.ProductVariantID,
                        principalTable: "ProductVariants",
                        principalColumn: "ProductVariantID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VendorProducts",
                columns: table => new
                {
                    VendorProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    VendorID = table.Column<int>(type: "int", nullable: false),
                    ProductVariantID = table.Column<int>(type: "int", nullable: false),
                    ReferencePriceUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OverridePrice = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    DeliveryDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorProducts", x => x.VendorProductID);
                    table.ForeignKey(
                        name: "FK_VendorProducts_ProductVariants",
                        column: x => x.ProductVariantID,
                        principalTable: "ProductVariants",
                        principalColumn: "ProductVariantID");
                    table.ForeignKey(
                        name: "FK_VendorProducts_Vendors",
                        column: x => x.VendorID,
                        principalTable: "Vendors",
                        principalColumn: "VendorID");
                    table.ForeignKey(
                        name: "FK_VendorProducts_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WishlistItems",
                columns: table => new
                {
                    WishlistItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WishlistID = table.Column<int>(type: "int", nullable: false),
                    ProductVariantID = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishlistItems", x => x.WishlistItemID);
                    table.ForeignKey(
                        name: "FK_WishlistItems_ProductVariants",
                        column: x => x.ProductVariantID,
                        principalTable: "ProductVariants",
                        principalColumn: "ProductVariantID");
                    table.ForeignKey(
                        name: "FK_WishlistItems_Wishlists",
                        column: x => x.WishlistID,
                        principalTable: "Wishlists",
                        principalColumn: "WishlistID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductWarningTranslations",
                columns: table => new
                {
                    ProductWarningTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductWarningID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Text = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductWarningTranslations", x => x.ProductWarningTranslationID);
                    table.ForeignKey(
                        name: "FK_ProductWarningTranslations_ProductWarnings",
                        column: x => x.ProductWarningID,
                        principalTable: "ProductWarnings",
                        principalColumn: "ProductWarningID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MenuItemTranslations",
                columns: table => new
                {
                    MenuItemTranslationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MenuItemID = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemTranslations", x => x.MenuItemTranslationID);
                    table.ForeignKey(
                        name: "FK_MenuItemTranslations_MenuItems",
                        column: x => x.MenuItemID,
                        principalTable: "MenuItems",
                        principalColumn: "MenuItemID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PaymentRefunds",
                columns: table => new
                {
                    PaymentRefundID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PaymentID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    BankAccountID = table.Column<int>(type: "int", nullable: true),
                    ClientWalletTransactionID = table.Column<int>(type: "int", nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedByMemberID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRefunds", x => x.PaymentRefundID);
                    table.ForeignKey(
                        name: "FK_PaymentRefunds_BankAccounts",
                        column: x => x.BankAccountID,
                        principalTable: "BankAccounts",
                        principalColumn: "BankAccountID");
                    table.ForeignKey(
                        name: "FK_PaymentRefunds_ClientWalletTransactions",
                        column: x => x.ClientWalletTransactionID,
                        principalTable: "ClientWalletTransactions",
                        principalColumn: "ClientWalletTransactionID");
                    table.ForeignKey(
                        name: "FK_PaymentRefunds_Members",
                        column: x => x.CreatedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_PaymentRefunds_Payments",
                        column: x => x.PaymentID,
                        principalTable: "Payments",
                        principalColumn: "PaymentID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    OrderItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    SourceWebsiteID = table.Column<int>(type: "int", nullable: false),
                    ProductVariantID = table.Column<int>(type: "int", nullable: false),
                    VendorProductID = table.Column<int>(type: "int", nullable: true),
                    VendorID = table.Column<int>(type: "int", nullable: true),
                    TitleSnapshot = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    SkuSnapshot = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitPriceUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitCostUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.OrderItemID);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
                    table.ForeignKey(
                        name: "FK_OrderItems_ProductVariants",
                        column: x => x.ProductVariantID,
                        principalTable: "ProductVariants",
                        principalColumn: "ProductVariantID");
                    table.ForeignKey(
                        name: "FK_OrderItems_VendorProducts",
                        column: x => x.VendorProductID,
                        principalTable: "VendorProducts",
                        principalColumn: "VendorProductID");
                    table.ForeignKey(
                        name: "FK_OrderItems_Vendors",
                        column: x => x.VendorID,
                        principalTable: "Vendors",
                        principalColumn: "VendorID");
                    table.ForeignKey(
                        name: "FK_OrderItems_Website1",
                        column: x => x.SourceWebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                    table.ForeignKey(
                        name: "FK_OrderItems_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    StockMovementID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    ProductVariantID = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitCostUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitSalePriceUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    CurrencyCode = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    ExchangeRateToUsd = table.Column<decimal>(type: "decimal(18,6)", nullable: false, comment: "website's local currency rate snapshotted at the moment of this stock entry/exit, so accounting and reports can be reconstructed in local currency at that point in time"),
                    SupplierID = table.Column<int>(type: "int", nullable: true),
                    OrderID = table.Column<int>(type: "int", nullable: true),
                    OrderItemID = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedByMemberID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.StockMovementID);
                    table.ForeignKey(
                        name: "FK_StockMovements_Currencies",
                        column: x => x.CurrencyCode,
                        principalTable: "Currencies",
                        principalColumn: "CurrencyCode");
                    table.ForeignKey(
                        name: "FK_StockMovements_Members",
                        column: x => x.CreatedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_StockMovements_OrderItems",
                        column: x => x.OrderItemID,
                        principalTable: "OrderItems",
                        principalColumn: "OrderItemID");
                    table.ForeignKey(
                        name: "FK_StockMovements_Orders",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
                    table.ForeignKey(
                        name: "FK_StockMovements_ProductVariants",
                        column: x => x.ProductVariantID,
                        principalTable: "ProductVariants",
                        principalColumn: "ProductVariantID");
                    table.ForeignKey(
                        name: "FK_StockMovements_Suppliers",
                        column: x => x.SupplierID,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierID");
                    table.ForeignKey(
                        name: "FK_StockMovements_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VendorCreditTransactions",
                columns: table => new
                {
                    VendorCreditTransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    VendorID = table.Column<int>(type: "int", nullable: false),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    AmountUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    BalanceAfterUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    SourceType = table.Column<byte>(type: "tinyint unsigned", nullable: false, comment: "1 = Grant, 2 = Sale, 3 = Adjustment, 4 = Refund"),
                    SourceOrderItemID = table.Column<int>(type: "int", nullable: true),
                    MirrorOrderID = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedByMemberID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorCreditTransactions", x => x.VendorCreditTransactionID);
                    table.ForeignKey(
                        name: "FK_VendorCreditTransactions_Members",
                        column: x => x.CreatedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_VendorCreditTransactions_OrderItems",
                        column: x => x.SourceOrderItemID,
                        principalTable: "OrderItems",
                        principalColumn: "OrderItemID");
                    table.ForeignKey(
                        name: "FK_VendorCreditTransactions_Orders",
                        column: x => x.MirrorOrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
                    table.ForeignKey(
                        name: "FK_VendorCreditTransactions_Vendors",
                        column: x => x.VendorID,
                        principalTable: "Vendors",
                        principalColumn: "VendorID");
                    table.ForeignKey(
                        name: "FK_VendorCreditTransactions_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SettlementItems",
                columns: table => new
                {
                    SettlementItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SettlementID = table.Column<int>(type: "int", nullable: false),
                    OrderItemID = table.Column<int>(type: "int", nullable: true),
                    StockMovementID = table.Column<int>(type: "int", nullable: true),
                    PaymentID = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Description = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementItems", x => x.SettlementItemID);
                    table.ForeignKey(
                        name: "FK_SettlementItems_OrderItems",
                        column: x => x.OrderItemID,
                        principalTable: "OrderItems",
                        principalColumn: "OrderItemID");
                    table.ForeignKey(
                        name: "FK_SettlementItems_Payments",
                        column: x => x.PaymentID,
                        principalTable: "Payments",
                        principalColumn: "PaymentID");
                    table.ForeignKey(
                        name: "FK_SettlementItems_Settlements",
                        column: x => x.SettlementID,
                        principalTable: "Settlements",
                        principalColumn: "SettlementID");
                    table.ForeignKey(
                        name: "FK_SettlementItems_StockMovements",
                        column: x => x.StockMovementID,
                        principalTable: "StockMovements",
                        principalColumn: "StockMovementID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifications_MemberID_IsRead_CreatedAt",
                table: "AdminNotifications",
                columns: new[] { "MemberID", "IsRead", "CreatedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifications_WebsiteID",
                table: "AdminNotifications",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeDefinitions_WebsiteID",
                table: "AttributeDefinitions",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeDefinitionTranslations_AttributeDefinitionID",
                table: "AttributeDefinitionTranslations",
                column: "AttributeDefinitionID");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeOptions_AttributeDefinitionID",
                table: "AttributeOptions",
                column: "AttributeDefinitionID");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeOptionTranslations_AttributeOptionID",
                table: "AttributeOptionTranslations",
                column: "AttributeOptionID");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_BankID",
                table: "BankAccounts",
                column: "BankID");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_CreatedByMemberId",
                table: "BankAccounts",
                column: "CreatedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_WebsiteID",
                table: "BankAccounts",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Banks_LogoFileID",
                table: "Banks",
                column: "LogoFileID");

            migrationBuilder.CreateIndex(
                name: "IX_Banks_WebsiteID",
                table: "Banks",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_LogoFileID",
                table: "Brands",
                column: "LogoFileID");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_WebsiteID",
                table: "Brands",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_BrandTranslations_BrandID",
                table: "BrandTranslations",
                column: "BrandID");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductVariantID",
                table: "CartItems",
                column: "ProductVariantID");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_VendorProductID",
                table: "CartItems",
                column: "VendorProductID");

            migrationBuilder.CreateIndex(
                name: "UQ_CartItems_CartID_ProductVariantID_VendorProductID",
                table: "CartItems",
                columns: new[] { "CartID", "ProductVariantID", "VendorProductID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_CouponID",
                table: "Carts",
                column: "CouponID");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_WebsiteClientID",
                table: "Carts",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_WebsiteID",
                table: "Carts",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryID",
                table: "Categories",
                column: "ParentCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_PostTypeID",
                table: "Categories",
                column: "PostTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_WebsiteID",
                table: "Categories",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryTranslations_CategoryID",
                table: "CategoryTranslations",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_ParentAccountID",
                table: "ChartOfAccounts",
                column: "ParentAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_WebsiteID",
                table: "ChartOfAccounts",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CountryID",
                table: "Cities",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_StateID",
                table: "Cities",
                column: "StateID");

            migrationBuilder.CreateIndex(
                name: "IX_CityTranslations_CityID",
                table: "CityTranslations",
                column: "CityID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientBankAccounts_BankID",
                table: "ClientBankAccounts",
                column: "BankID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientBankAccounts_WebsiteClientID",
                table: "ClientBankAccounts",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientBankAccounts_WebsiteID",
                table: "ClientBankAccounts",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientWallets_WebsiteID",
                table: "ClientWallets",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "UQ_ClientWallets_WebsiteClientID",
                table: "ClientWallets",
                column: "WebsiteClientID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientWalletTransactions_ClientWalletID",
                table: "ClientWalletTransactions",
                column: "ClientWalletID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientWalletTransactions_CreatedByMemberID",
                table: "ClientWalletTransactions",
                column: "CreatedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientWalletTransactions_WebsiteID",
                table: "ClientWalletTransactions",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientWalletWithdrawals_ClientBankAccountID",
                table: "ClientWalletWithdrawals",
                column: "ClientBankAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientWalletWithdrawals_ClientWalletID",
                table: "ClientWalletWithdrawals",
                column: "ClientWalletID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientWalletWithdrawals_ReviewedByMemberID",
                table: "ClientWalletWithdrawals",
                column: "ReviewedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientWalletWithdrawals_WebsiteClientID",
                table: "ClientWalletWithdrawals",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_ClientWalletWithdrawals_WebsiteID",
                table: "ClientWalletWithdrawals",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_ContactUsMessages_WebsiteID",
                table: "ContactUsMessages",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_CountryTranslations_CountryID",
                table: "CountryTranslations",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_CouponRedemptions_CouponID",
                table: "CouponRedemptions",
                column: "CouponID");

            migrationBuilder.CreateIndex(
                name: "IX_CouponRedemptions_WebsiteClientID",
                table: "CouponRedemptions",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "UQ_CouponRedemptions_OrderID",
                table: "CouponRedemptions",
                column: "OrderID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_CreatedByMemberID",
                table: "Coupons",
                column: "CreatedByMemberID");

            migrationBuilder.CreateIndex(
                name: "UQ_Coupons_WebsiteID_Code",
                table: "Coupons",
                columns: new[] { "WebsiteID", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRates_CurrencyCode",
                table: "CurrencyRates",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRates_WebsiteID",
                table: "CurrencyRates",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_EmailAccounts_WebsiteID",
                table: "EmailAccounts",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSubscribes_MemberID",
                table: "EmailSubscribes",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSubscribes_WebsiteID",
                table: "EmailSubscribes",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_WebsiteID_TemplateKey",
                table: "EmailTemplates",
                columns: new[] { "WebsiteID", "TemplateKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateTranslations_EmailTemplateID_LanguageCode",
                table: "EmailTemplateTranslations",
                columns: new[] { "EmailTemplateID", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileFolders_ParentFolderID",
                table: "FileFolders",
                column: "ParentFolderID");

            migrationBuilder.CreateIndex(
                name: "IX_FileFolders_WebsiteID",
                table: "FileFolders",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecords_FileFolderID",
                table: "FileRecords",
                column: "FileFolderID");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecords_UploaderMemberID",
                table: "FileRecords",
                column: "UploaderMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecords_WebsiteClientID",
                table: "FileRecords",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecords_WebsiteID",
                table: "FileRecords",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecords_WebsiteStorageSettingsID",
                table: "FileRecords",
                column: "WebsiteStorageSettingsID");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecordTags_FileRecordID",
                table: "FileRecordTags",
                column: "FileRecordID");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecordTags_FileTagID",
                table: "FileRecordTags",
                column: "FileTagID");

            migrationBuilder.CreateIndex(
                name: "IX_FileTags_WebsiteID",
                table: "FileTags",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_FormFieldOptions_FormFieldID",
                table: "FormFieldOptions",
                column: "FormFieldID");

            migrationBuilder.CreateIndex(
                name: "IX_FormFields_FormID",
                table: "FormFields",
                column: "FormID");

            migrationBuilder.CreateIndex(
                name: "IX_FormResponses_FormID",
                table: "FormResponses",
                column: "FormID");

            migrationBuilder.CreateIndex(
                name: "IX_FormResponses_WebsiteClientID",
                table: "FormResponses",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_FormResponseValues_FormFieldID",
                table: "FormResponseValues",
                column: "FormFieldID");

            migrationBuilder.CreateIndex(
                name: "IX_FormResponseValues_FormResponseID",
                table: "FormResponseValues",
                column: "FormResponseID");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_WebsiteID",
                table: "Forms",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ProductVariantID",
                table: "InventoryItems",
                column: "ProductVariantID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_WebsiteID",
                table: "InventoryItems",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_WebsiteID",
                table: "JournalEntries",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_ChartOfAccountID",
                table: "JournalEntryLines",
                column: "ChartOfAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_JournalEntryID",
                table: "JournalEntryLines",
                column: "JournalEntryID");

            migrationBuilder.CreateIndex(
                name: "UQ_Languages_Admin_LanguageCode",
                table: "Languages",
                column: "LanguageCode",
                unique: true,
                filter: "([WebsiteID] IS NULL)");

            migrationBuilder.CreateIndex(
                name: "UQ_Languages_WebsiteID_LanguageCode",
                table: "Languages",
                columns: new[] { "WebsiteID", "LanguageCode" },
                unique: true,
                filter: "([WebsiteID] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationKeys_WebsiteID",
                table: "LocalizationKeys",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "UQ_LocalizationKeys_Admin_ItemKey",
                table: "LocalizationKeys",
                column: "ItemKey",
                unique: true,
                filter: "([WebsiteID] IS NULL)");

            migrationBuilder.CreateIndex(
                name: "UQ_LocalizationKeys_WebsiteID_ItemKey",
                table: "LocalizationKeys",
                columns: new[] { "WebsiteID", "ItemKey" },
                unique: true,
                filter: "([WebsiteID] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationValues_LocalizationKeyID",
                table: "LocalizationValues",
                column: "LocalizationKeyID");

            migrationBuilder.CreateIndex(
                name: "IX_LoginTries_WebsiteID",
                table: "LoginTries",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_MediaSetItems_FileID",
                table: "MediaSetItems",
                column: "FileID");

            migrationBuilder.CreateIndex(
                name: "IX_MediaSetItems_MediaSetID",
                table: "MediaSetItems",
                column: "MediaSetID");

            migrationBuilder.CreateIndex(
                name: "IX_MediaSetItems_VideoThumbnailFileID",
                table: "MediaSetItems",
                column: "VideoThumbnailFileID");

            migrationBuilder.CreateIndex(
                name: "IX_MediaSets_WebsiteID",
                table: "MediaSets",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_MemberForgetPasswords_MemberID",
                table: "MemberForgetPasswords",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_Members_AvatarID",
                table: "Members",
                column: "AvatarID");

            migrationBuilder.CreateIndex(
                name: "IX_Members_PolicyID",
                table: "Members",
                column: "PolicyID");

            migrationBuilder.CreateIndex(
                name: "IX_Members_WebsiteID",
                table: "Members",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_BrandID",
                table: "MenuItems",
                column: "BrandID");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_CategoryID",
                table: "MenuItems",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_MenuID",
                table: "MenuItems",
                column: "MenuID");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_PageID",
                table: "MenuItems",
                column: "PageID");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ParentItemID",
                table: "MenuItems",
                column: "ParentItemID");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_PostID",
                table: "MenuItems",
                column: "PostID");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ProductCategoryID",
                table: "MenuItems",
                column: "ProductCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ProductID",
                table: "MenuItems",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_VendorID",
                table: "MenuItems",
                column: "VendorID");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemTranslations_MenuItemID",
                table: "MenuItemTranslations",
                column: "MenuItemID");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_WebsiteID",
                table: "Menus",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderID",
                table: "OrderItems",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductVariantID",
                table: "OrderItems",
                column: "ProductVariantID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_SourceWebsiteID",
                table: "OrderItems",
                column: "SourceWebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_VendorID",
                table: "OrderItems",
                column: "VendorID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_VendorProductID",
                table: "OrderItems",
                column: "VendorProductID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_WebsiteID",
                table: "OrderItems",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CouponID",
                table: "Orders",
                column: "CouponID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedByMemberID",
                table: "Orders",
                column: "CreatedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CurrencyCode",
                table: "Orders",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingMethodID",
                table: "Orders",
                column: "ShippingMethodID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_WebsiteClientAddressID",
                table: "Orders",
                column: "WebsiteClientAddressID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_WebsiteClientID",
                table: "Orders",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_WebsiteID",
                table: "Orders",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_CreatedByMemberID",
                table: "OrderStatusHistories",
                column: "CreatedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_OrderID",
                table: "OrderStatusHistories",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_CreatedByMemberID",
                table: "Pages",
                column: "CreatedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_ParentPageID",
                table: "Pages",
                column: "ParentPageID");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_WebsiteID",
                table: "Pages",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_PageTranslations_PageID",
                table: "PageTranslations",
                column: "PageID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGateways_WebsiteID",
                table: "PaymentGateways",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_BankAccountID",
                table: "PaymentRefunds",
                column: "BankAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_ClientWalletTransactionID",
                table: "PaymentRefunds",
                column: "ClientWalletTransactionID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_CreatedByMemberID",
                table: "PaymentRefunds",
                column: "CreatedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_PaymentID",
                table: "PaymentRefunds",
                column: "PaymentID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BankAccountID",
                table: "Payments",
                column: "BankAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ClientWalletTransactionID",
                table: "Payments",
                column: "ClientWalletTransactionID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CurrencyCode",
                table: "Payments",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderID",
                table: "Payments",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentGatewayID",
                table: "Payments",
                column: "PaymentGatewayID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ReceiptFileID",
                table: "Payments",
                column: "ReceiptFileID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_VerifiedByMemberID",
                table: "Payments",
                column: "VerifiedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_WebsiteClientID",
                table: "Payments",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_WebsiteID",
                table: "Payments",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_WebsiteID",
                table: "Policies",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyRoles_PolicyID",
                table: "PolicyRoles",
                column: "PolicyID");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyRoles_RoleID",
                table: "PolicyRoles",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_PostCategories_CategoryID",
                table: "PostCategories",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_AuthorMemberID",
                table: "Posts",
                column: "AuthorMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_FeaturedImageFileID",
                table: "Posts",
                column: "FeaturedImageFileID");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_PostTypeID",
                table: "Posts",
                column: "PostTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_WebsiteID",
                table: "Posts",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_PostTags_TagID",
                table: "PostTags",
                column: "TagID");

            migrationBuilder.CreateIndex(
                name: "IX_PostTranslations_PostID",
                table: "PostTranslations",
                column: "PostID");

            migrationBuilder.CreateIndex(
                name: "IX_PostTypes_WebsiteID",
                table: "PostTypes",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAnswers_ProductQuestionID",
                table: "ProductAnswers",
                column: "ProductQuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAnswers_VendorID",
                table: "ProductAnswers",
                column: "VendorID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAnswers_WebsiteClientID",
                table: "ProductAnswers",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeValues_AttributeDefinitionID",
                table: "ProductAttributeValues",
                column: "AttributeDefinitionID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeValues_AttributeOptionID",
                table: "ProductAttributeValues",
                column: "AttributeOptionID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeValues_ProductID",
                table: "ProductAttributeValues",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeValueTranslations_ProductAttributeValueID",
                table: "ProductAttributeValueTranslations",
                column: "ProductAttributeValueID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ImageFileID",
                table: "ProductCategories",
                column: "ImageFileID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ParentCategoryID",
                table: "ProductCategories",
                column: "ParentCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_WebsiteID",
                table: "ProductCategories",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryMaps_ProductCategoryID",
                table: "ProductCategoryMaps",
                column: "ProductCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryRelations_RelatedProductCategoryID",
                table: "ProductCategoryRelations",
                column: "RelatedProductCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryTranslations_ProductCategoryID",
                table: "ProductCategoryTranslations",
                column: "ProductCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMedia_MediaSetID",
                table: "ProductMedia",
                column: "MediaSetID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuestions_ProductID",
                table: "ProductQuestions",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuestions_WebsiteClientID",
                table: "ProductQuestions",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuestions_WebsiteID",
                table: "ProductQuestions",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRelations_RelatedProductID",
                table: "ProductRelations",
                column: "RelatedProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_ProductID",
                table: "ProductReviews",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_ProductVariantID",
                table: "ProductReviews",
                column: "ProductVariantID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_WebsiteClientID",
                table: "ProductReviews",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_WebsiteID",
                table: "ProductReviews",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandID",
                table: "Products",
                column: "BrandID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedByMemberId",
                table: "Products",
                column: "CreatedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_FeaturedImageFileID",
                table: "Products",
                column: "FeaturedImageFileID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_WebsiteID",
                table: "Products",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTranslations_ProductID",
                table: "ProductTranslations",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantPriceHistories_ChangedByMemberId",
                table: "ProductVariantPriceHistories",
                column: "ChangedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantPriceHistories_ProductVariantID_RecordedAt",
                table: "ProductVariantPriceHistories",
                columns: new[] { "ProductVariantID", "RecordedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ImageFileID",
                table: "ProductVariants",
                column: "ImageFileID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductID",
                table: "ProductVariants",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_WebsiteID",
                table: "ProductVariants",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductWarnings_ProductID",
                table: "ProductWarnings",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductWarningTranslations_ProductWarningID",
                table: "ProductWarningTranslations",
                column: "ProductWarningID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductWarranties_ProductID",
                table: "ProductWarranties",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductWarranties_WarrantyID",
                table: "ProductWarranties",
                column: "WarrantyID");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementItems_OrderItemID",
                table: "SettlementItems",
                column: "OrderItemID");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementItems_PaymentID",
                table: "SettlementItems",
                column: "PaymentID");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementItems_SettlementID",
                table: "SettlementItems",
                column: "SettlementID");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementItems_StockMovementID",
                table: "SettlementItems",
                column: "StockMovementID");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_ApprovedByMemberID",
                table: "Settlements",
                column: "ApprovedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_BankAccountID",
                table: "Settlements",
                column: "BankAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_CreatedByMemberID",
                table: "Settlements",
                column: "CreatedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_CurrencyCode",
                table: "Settlements",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_SupplierID",
                table: "Settlements",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_TargetWebsiteID",
                table: "Settlements",
                column: "TargetWebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_VendorID",
                table: "Settlements",
                column: "VendorID");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_WebsiteID",
                table: "Settlements",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingMethods_WebsiteID",
                table: "ShippingMethods",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingRates_CityID",
                table: "ShippingRates",
                column: "CityID");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingRates_CountryID",
                table: "ShippingRates",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingRates_ShippingMethodID",
                table: "ShippingRates",
                column: "ShippingMethodID");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingRates_StateID",
                table: "ShippingRates",
                column: "StateID");

            migrationBuilder.CreateIndex(
                name: "IX_Slideshows_WebsiteID",
                table: "Slideshows",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_SlideshowSlides_FileID",
                table: "SlideshowSlides",
                column: "FileID");

            migrationBuilder.CreateIndex(
                name: "IX_SlideshowSlides_MobileFileID",
                table: "SlideshowSlides",
                column: "MobileFileID");

            migrationBuilder.CreateIndex(
                name: "IX_SlideshowSlides_SlideshowID",
                table: "SlideshowSlides",
                column: "SlideshowID");

            migrationBuilder.CreateIndex(
                name: "IX_States_CountryID",
                table: "States",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_StateTranslations_StateID",
                table: "StateTranslations",
                column: "StateID");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_CreatedByMemberID",
                table: "StockMovements",
                column: "CreatedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_CurrencyCode",
                table: "StockMovements",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_OrderID",
                table: "StockMovements",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_OrderItemID",
                table: "StockMovements",
                column: "OrderItemID");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductVariantID",
                table: "StockMovements",
                column: "ProductVariantID");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_SupplierID",
                table: "StockMovements",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_WebsiteID",
                table: "StockMovements",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_WebsiteID",
                table: "Suppliers",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_WebsiteID",
                table: "Tags",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_TagTranslations_TagID",
                table: "TagTranslations",
                column: "TagID");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_CountryID",
                table: "TaxRates",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_StateID",
                table: "TaxRates",
                column: "StateID");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_WebsiteID",
                table: "TaxRates",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_VariantAttributeValues_AttributeDefinitionID",
                table: "VariantAttributeValues",
                column: "AttributeDefinitionID");

            migrationBuilder.CreateIndex(
                name: "IX_VariantAttributeValues_AttributeOptionID",
                table: "VariantAttributeValues",
                column: "AttributeOptionID");

            migrationBuilder.CreateIndex(
                name: "IX_VendorCreditTransactions_CreatedByMemberID",
                table: "VendorCreditTransactions",
                column: "CreatedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_VendorCreditTransactions_MirrorOrderID",
                table: "VendorCreditTransactions",
                column: "MirrorOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_VendorCreditTransactions_SourceOrderItemID",
                table: "VendorCreditTransactions",
                column: "SourceOrderItemID");

            migrationBuilder.CreateIndex(
                name: "IX_VendorCreditTransactions_VendorID",
                table: "VendorCreditTransactions",
                column: "VendorID");

            migrationBuilder.CreateIndex(
                name: "IX_VendorCreditTransactions_WebsiteID",
                table: "VendorCreditTransactions",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_VendorProducts_ProductVariantID",
                table: "VendorProducts",
                column: "ProductVariantID");

            migrationBuilder.CreateIndex(
                name: "IX_VendorProducts_VendorID",
                table: "VendorProducts",
                column: "VendorID");

            migrationBuilder.CreateIndex(
                name: "IX_VendorProducts_WebsiteID",
                table: "VendorProducts",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_LinkedWebsiteID",
                table: "Vendors",
                column: "LinkedWebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_LogoFileID",
                table: "Vendors",
                column: "LogoFileID");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_MemberID",
                table: "Vendors",
                column: "MemberID",
                unique: true,
                filter: "([MemberID] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_WebsiteID",
                table: "Vendors",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_VendorTranslations_VendorID",
                table: "VendorTranslations",
                column: "VendorID");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_WebsiteID",
                table: "Warranties",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyTranslations_WarrantyID",
                table: "WarrantyTranslations",
                column: "WarrantyID");

            migrationBuilder.CreateIndex(
                name: "UQ_WebsiteCaptchaSettings_WebsiteID",
                table: "WebsiteCaptchaSettings",
                column: "WebsiteID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteClientAddresses_CityId",
                table: "WebsiteClientAddresses",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteClientAddresses_CountryId",
                table: "WebsiteClientAddresses",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteClientAddresses_WebsiteClientID",
                table: "WebsiteClientAddresses",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteClientForgetPasswords_WebsiteClientID",
                table: "WebsiteClientForgetPasswords",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteClients_AvatarID",
                table: "WebsiteClients",
                column: "AvatarID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteClients_WebsiteID",
                table: "WebsiteClients",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "UQ_WebsiteFeatures_WebsiteID_FeatureKey",
                table: "WebsiteFeatures",
                columns: new[] { "WebsiteID", "FeatureKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteIPs_WebsiteID",
                table: "WebsiteIPs",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteRedirects_WebsiteID",
                table: "WebsiteRedirects",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Websites_DefaultCurrencyCode",
                table: "Websites",
                column: "DefaultCurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_Websites_FaveIconFileID",
                table: "Websites",
                column: "FaveIconFileID");

            migrationBuilder.CreateIndex(
                name: "IX_Websites_LogoFileID",
                table: "Websites",
                column: "LogoFileID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteScripts_WebsiteID",
                table: "WebsiteScripts",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteSeoSettings_WebsiteID",
                table: "WebsiteSeoSettings",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteSocialLinks_WebsiteID",
                table: "WebsiteSocialLinks",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteStorageSettings_WebsiteID",
                table: "WebsiteStorageSettings",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteThemes_WebsiteID",
                table: "WebsiteThemes",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteThemes_WebsiteID_Slug",
                table: "WebsiteThemes",
                columns: new[] { "WebsiteID", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteWatermarkSettings_WatermarkFileID",
                table: "WebsiteWatermarkSettings",
                column: "WatermarkFileID");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteWatermarkSettings_WebsiteID",
                table: "WebsiteWatermarkSettings",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_ProductVariantID",
                table: "WishlistItems",
                column: "ProductVariantID");

            migrationBuilder.CreateIndex(
                name: "UQ_WishlistItems_WishlistID_ProductVariantID",
                table: "WishlistItems",
                columns: new[] { "WishlistID", "ProductVariantID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wishlists_WebsiteID",
                table: "Wishlists",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "UQ_Wishlists_WebsiteClientID",
                table: "Wishlists",
                column: "WebsiteClientID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AdminNotifications_Members",
                table: "AdminNotifications",
                column: "MemberID",
                principalTable: "Members",
                principalColumn: "MemberID");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminNotifications_Websites",
                table: "AdminNotifications",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_AttributeDefinitions_Websites",
                table: "AttributeDefinitions",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_Banks",
                table: "BankAccounts",
                column: "BankID",
                principalTable: "Banks",
                principalColumn: "BankID");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_Members",
                table: "BankAccounts",
                column: "CreatedByMemberId",
                principalTable: "Members",
                principalColumn: "MemberID");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_Websites",
                table: "BankAccounts",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_Banks_FileRecords",
                table: "Banks",
                column: "LogoFileID",
                principalTable: "FileRecords",
                principalColumn: "FileRecordID");

            migrationBuilder.AddForeignKey(
                name: "FK_Banks_Websites",
                table: "Banks",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_Brands_FileRecords",
                table: "Brands",
                column: "LogoFileID",
                principalTable: "FileRecords",
                principalColumn: "FileRecordID");

            migrationBuilder.AddForeignKey(
                name: "FK_Brands_Websites",
                table: "Brands",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Carts",
                table: "CartItems",
                column: "CartID",
                principalTable: "Carts",
                principalColumn: "CartID");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_ProductVariants",
                table: "CartItems",
                column: "ProductVariantID",
                principalTable: "ProductVariants",
                principalColumn: "ProductVariantID");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_VendorProducts",
                table: "CartItems",
                column: "VendorProductID",
                principalTable: "VendorProducts",
                principalColumn: "VendorProductID");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Coupons",
                table: "Carts",
                column: "CouponID",
                principalTable: "Coupons",
                principalColumn: "CouponID");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_WebsiteClients",
                table: "Carts",
                column: "WebsiteClientID",
                principalTable: "WebsiteClients",
                principalColumn: "WebsiteClientID");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Websites",
                table: "Carts",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_PostTypes",
                table: "Categories",
                column: "PostTypeID",
                principalTable: "PostTypes",
                principalColumn: "PostTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Websites",
                table: "Categories",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_ChartOfAccounts_Websites",
                table: "ChartOfAccounts",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientBankAccounts_WebsiteClients",
                table: "ClientBankAccounts",
                column: "WebsiteClientID",
                principalTable: "WebsiteClients",
                principalColumn: "WebsiteClientID");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientBankAccounts_Websites",
                table: "ClientBankAccounts",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientWallets_WebsiteClients",
                table: "ClientWallets",
                column: "WebsiteClientID",
                principalTable: "WebsiteClients",
                principalColumn: "WebsiteClientID");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientWallets_Websites",
                table: "ClientWallets",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientWalletTransactions_Members",
                table: "ClientWalletTransactions",
                column: "CreatedByMemberID",
                principalTable: "Members",
                principalColumn: "MemberID");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientWalletTransactions_Websites",
                table: "ClientWalletTransactions",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientWalletWithdrawals_Members",
                table: "ClientWalletWithdrawals",
                column: "ReviewedByMemberID",
                principalTable: "Members",
                principalColumn: "MemberID");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientWalletWithdrawals_WebsiteClients",
                table: "ClientWalletWithdrawals",
                column: "WebsiteClientID",
                principalTable: "WebsiteClients",
                principalColumn: "WebsiteClientID");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientWalletWithdrawals_Websites",
                table: "ClientWalletWithdrawals",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactUsMessages_Websites",
                table: "ContactUsMessages",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_CouponRedemptions_Coupons",
                table: "CouponRedemptions",
                column: "CouponID",
                principalTable: "Coupons",
                principalColumn: "CouponID");

            migrationBuilder.AddForeignKey(
                name: "FK_CouponRedemptions_Orders",
                table: "CouponRedemptions",
                column: "OrderID",
                principalTable: "Orders",
                principalColumn: "OrderID");

            migrationBuilder.AddForeignKey(
                name: "FK_CouponRedemptions_WebsiteClients",
                table: "CouponRedemptions",
                column: "WebsiteClientID",
                principalTable: "WebsiteClients",
                principalColumn: "WebsiteClientID");

            migrationBuilder.AddForeignKey(
                name: "FK_Coupons_Members",
                table: "Coupons",
                column: "CreatedByMemberID",
                principalTable: "Members",
                principalColumn: "MemberID");

            migrationBuilder.AddForeignKey(
                name: "FK_Coupons_Websites",
                table: "Coupons",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrencyRates_Websites",
                table: "CurrencyRates",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailAccounts_Websites",
                table: "EmailAccounts",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSubscribes_Members",
                table: "EmailSubscribes",
                column: "MemberID",
                principalTable: "Members",
                principalColumn: "MemberID");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSubscribes_Websites",
                table: "EmailSubscribes",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailTemplates_Websites",
                table: "EmailTemplates",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_FileFolders_Websites",
                table: "FileFolders",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_FileRecords_Members",
                table: "FileRecords",
                column: "UploaderMemberID",
                principalTable: "Members",
                principalColumn: "MemberID");

            migrationBuilder.AddForeignKey(
                name: "FK_FileRecords_WebsiteClients",
                table: "FileRecords",
                column: "WebsiteClientID",
                principalTable: "WebsiteClients",
                principalColumn: "WebsiteClientID");

            migrationBuilder.AddForeignKey(
                name: "FK_FileRecords_WebsiteStorageSettings",
                table: "FileRecords",
                column: "WebsiteStorageSettingsID",
                principalTable: "WebsiteStorageSettings",
                principalColumn: "WebsiteStorageSettingsID");

            migrationBuilder.AddForeignKey(
                name: "FK_FileRecords_Websites",
                table: "FileRecords",
                column: "WebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileRecords_Members",
                table: "FileRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_FileFolders_Websites",
                table: "FileFolders");

            migrationBuilder.DropForeignKey(
                name: "FK_FileRecords_Websites",
                table: "FileRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_WebsiteClients_Websites",
                table: "WebsiteClients");

            migrationBuilder.DropForeignKey(
                name: "FK_WebsiteStorageSettings_Websites",
                table: "WebsiteStorageSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_WebsiteClients_FileRecords",
                table: "WebsiteClients");

            migrationBuilder.DropTable(
                name: "AdminNotifications");

            migrationBuilder.DropTable(
                name: "AttributeDefinitionTranslations");

            migrationBuilder.DropTable(
                name: "AttributeOptionTranslations");

            migrationBuilder.DropTable(
                name: "BrandTranslations");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "CategoryTranslations");

            migrationBuilder.DropTable(
                name: "CityTranslations");

            migrationBuilder.DropTable(
                name: "ClientWalletWithdrawals");

            migrationBuilder.DropTable(
                name: "ContactUsMessages");

            migrationBuilder.DropTable(
                name: "CountryTranslations");

            migrationBuilder.DropTable(
                name: "CouponRedemptions");

            migrationBuilder.DropTable(
                name: "CurrencyRates");

            migrationBuilder.DropTable(
                name: "EmailAccounts");

            migrationBuilder.DropTable(
                name: "EmailSubscribes");

            migrationBuilder.DropTable(
                name: "EmailTemplateTranslations");

            migrationBuilder.DropTable(
                name: "FileRecordTags");

            migrationBuilder.DropTable(
                name: "FormFieldOptions");

            migrationBuilder.DropTable(
                name: "FormResponseValues");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "JournalEntryLines");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "LocalizationValues");

            migrationBuilder.DropTable(
                name: "LoginTries");

            migrationBuilder.DropTable(
                name: "MediaSetItems");

            migrationBuilder.DropTable(
                name: "MemberForgetPasswords");

            migrationBuilder.DropTable(
                name: "MenuItemTranslations");

            migrationBuilder.DropTable(
                name: "OrderStatusHistories");

            migrationBuilder.DropTable(
                name: "PageTranslations");

            migrationBuilder.DropTable(
                name: "PaymentRefunds");

            migrationBuilder.DropTable(
                name: "PolicyRoles");

            migrationBuilder.DropTable(
                name: "PostCategories");

            migrationBuilder.DropTable(
                name: "PostTags");

            migrationBuilder.DropTable(
                name: "PostTranslations");

            migrationBuilder.DropTable(
                name: "ProductAnswers");

            migrationBuilder.DropTable(
                name: "ProductAttributeValueTranslations");

            migrationBuilder.DropTable(
                name: "ProductCategoryMaps");

            migrationBuilder.DropTable(
                name: "ProductCategoryRelations");

            migrationBuilder.DropTable(
                name: "ProductCategoryTranslations");

            migrationBuilder.DropTable(
                name: "ProductMedia");

            migrationBuilder.DropTable(
                name: "ProductRelations");

            migrationBuilder.DropTable(
                name: "ProductReviews");

            migrationBuilder.DropTable(
                name: "ProductTranslations");

            migrationBuilder.DropTable(
                name: "ProductVariantPriceHistories");

            migrationBuilder.DropTable(
                name: "ProductWarningTranslations");

            migrationBuilder.DropTable(
                name: "ProductWarranties");

            migrationBuilder.DropTable(
                name: "SettlementItems");

            migrationBuilder.DropTable(
                name: "ShippingRates");

            migrationBuilder.DropTable(
                name: "SlideshowSlides");

            migrationBuilder.DropTable(
                name: "StateTranslations");

            migrationBuilder.DropTable(
                name: "TagTranslations");

            migrationBuilder.DropTable(
                name: "TaxRates");

            migrationBuilder.DropTable(
                name: "VariantAttributeValues");

            migrationBuilder.DropTable(
                name: "VendorCreditTransactions");

            migrationBuilder.DropTable(
                name: "VendorTranslations");

            migrationBuilder.DropTable(
                name: "WarrantyTranslations");

            migrationBuilder.DropTable(
                name: "WebsiteCaptchaSettings");

            migrationBuilder.DropTable(
                name: "WebsiteClientForgetPasswords");

            migrationBuilder.DropTable(
                name: "WebsiteFeatures");

            migrationBuilder.DropTable(
                name: "WebsiteIPs");

            migrationBuilder.DropTable(
                name: "WebsiteRedirects");

            migrationBuilder.DropTable(
                name: "WebsiteScripts");

            migrationBuilder.DropTable(
                name: "WebsiteSeoSettings");

            migrationBuilder.DropTable(
                name: "WebsiteSocialLinks");

            migrationBuilder.DropTable(
                name: "WebsiteThemes");

            migrationBuilder.DropTable(
                name: "WebsiteWatermarkSettings");

            migrationBuilder.DropTable(
                name: "WishlistItems");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "ClientBankAccounts");

            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "FileTags");

            migrationBuilder.DropTable(
                name: "FormFields");

            migrationBuilder.DropTable(
                name: "FormResponses");

            migrationBuilder.DropTable(
                name: "ChartOfAccounts");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "LocalizationKeys");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "ProductQuestions");

            migrationBuilder.DropTable(
                name: "ProductAttributeValues");

            migrationBuilder.DropTable(
                name: "MediaSets");

            migrationBuilder.DropTable(
                name: "ProductWarnings");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Settlements");

            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "Slideshows");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Warranties");

            migrationBuilder.DropTable(
                name: "Wishlists");

            migrationBuilder.DropTable(
                name: "Forms");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Menus");

            migrationBuilder.DropTable(
                name: "Pages");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "AttributeOptions");

            migrationBuilder.DropTable(
                name: "ClientWalletTransactions");

            migrationBuilder.DropTable(
                name: "PaymentGateways");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "PostTypes");

            migrationBuilder.DropTable(
                name: "AttributeDefinitions");

            migrationBuilder.DropTable(
                name: "ClientWallets");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "VendorProducts");

            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropTable(
                name: "ShippingMethods");

            migrationBuilder.DropTable(
                name: "WebsiteClientAddresses");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropTable(
                name: "Vendors");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "States");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Policies");

            migrationBuilder.DropTable(
                name: "Websites");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "FileRecords");

            migrationBuilder.DropTable(
                name: "FileFolders");

            migrationBuilder.DropTable(
                name: "WebsiteClients");

            migrationBuilder.DropTable(
                name: "WebsiteStorageSettings");
        }
    }
}
