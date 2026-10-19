using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class WidenSettingSecretColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SettingsJSON",
                table: "WebsiteWhatsAppSettings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "StorageSettingsJSON",
                table: "WebsiteStorageSettings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "SettingsJSON",
                table: "WebsiteSmsSettings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "TurnstileSecretKey",
                table: "WebsiteCaptchaSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SettingsJSON",
                table: "PaymentGateways",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MerchantID",
                table: "PaymentGateways",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApiSecret",
                table: "PaymentGateways",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApiKey",
                table: "PaymentGateways",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            // Legacy databases were baselined at InitialCreate before marketplace tables existed.
            // Altering a missing MarketplaceChannels fails the whole sync.
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[MarketplaceChannels]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[MarketplaceChannels] (
                        [MarketplaceChannelID] int NOT NULL IDENTITY,
                        [WebsiteID] int NOT NULL,
                        [Provider] varchar(50) NOT NULL,
                        [Title] nvarchar(150) NOT NULL,
                        [SettingsJSON] nvarchar(max) NOT NULL,
                        [FeedToken] varchar(64) NOT NULL,
                        [IncludeAllProducts] bit NOT NULL CONSTRAINT [DF_MarketplaceChannels_IncludeAllProducts] DEFAULT CAST(1 AS bit),
                        [IsActive] bit NOT NULL,
                        [SortOrder] int NOT NULL,
                        [SyncIntervalMinutes] int NOT NULL,
                        [LastSyncAt] datetime2(0) NULL,
                        [LastSyncStatus] tinyint NULL,
                        [LastSyncMessage] nvarchar(1000) NULL,
                        [CreatedAt] datetime2(0) NOT NULL,
                        CONSTRAINT [PK_MarketplaceChannels] PRIMARY KEY ([MarketplaceChannelID]),
                        CONSTRAINT [FK_MarketplaceChannels_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
                    );
                    CREATE INDEX [IX_MarketplaceChannels_WebsiteID] ON [dbo].[MarketplaceChannels] ([WebsiteID]);
                    CREATE UNIQUE INDEX [UQ_MarketplaceChannels_FeedToken] ON [dbo].[MarketplaceChannels] ([FeedToken]);
                END

                IF OBJECT_ID(N'[dbo].[MarketplaceSyncLogs]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[MarketplaceSyncLogs] (
                        [MarketplaceSyncLogID] int NOT NULL IDENTITY,
                        [MarketplaceChannelID] int NOT NULL,
                        [StartedAt] datetime2(0) NOT NULL,
                        [FinishedAt] datetime2(0) NULL,
                        [Status] tinyint NOT NULL,
                        [TriggeredBy] varchar(20) NOT NULL,
                        [ItemCount] int NOT NULL,
                        [FailedCount] int NOT NULL,
                        [Message] nvarchar(1000) NULL,
                        CONSTRAINT [PK_MarketplaceSyncLogs] PRIMARY KEY ([MarketplaceSyncLogID]),
                        CONSTRAINT [FK_MarketplaceSyncLogs_MarketplaceChannels] FOREIGN KEY ([MarketplaceChannelID]) REFERENCES [dbo].[MarketplaceChannels] ([MarketplaceChannelID]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_MarketplaceSyncLogs_Channel] ON [dbo].[MarketplaceSyncLogs] ([MarketplaceChannelID], [MarketplaceSyncLogID]);
                END

                IF OBJECT_ID(N'[dbo].[MarketplaceChannelCategories]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[MarketplaceChannelCategories] (
                        [MarketplaceChannelCategoryID] int NOT NULL IDENTITY,
                        [MarketplaceChannelID] int NOT NULL,
                        [ProductCategoryID] int NOT NULL,
                        [IsIncluded] bit NOT NULL CONSTRAINT [DF_MarketplaceChannelCategories_IsIncluded] DEFAULT CAST(1 AS bit),
                        [RemoteCategoryID] nvarchar(100) NULL,
                        [RemoteCategoryPath] nvarchar(400) NULL,
                        CONSTRAINT [PK_MarketplaceChannelCategories] PRIMARY KEY ([MarketplaceChannelCategoryID]),
                        CONSTRAINT [FK_MarketplaceChannelCategories_MarketplaceChannels] FOREIGN KEY ([MarketplaceChannelID]) REFERENCES [dbo].[MarketplaceChannels] ([MarketplaceChannelID]) ON DELETE CASCADE,
                        CONSTRAINT [FK_MarketplaceChannelCategories_ProductCategories] FOREIGN KEY ([ProductCategoryID]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryID]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_MarketplaceChannelCategories_ProductCategoryID] ON [dbo].[MarketplaceChannelCategories] ([ProductCategoryID]);
                    CREATE UNIQUE INDEX [UQ_MarketplaceChannelCategories_Channel_Category] ON [dbo].[MarketplaceChannelCategories] ([MarketplaceChannelID], [ProductCategoryID]);
                END

                IF OBJECT_ID(N'[dbo].[MarketplaceChannelProducts]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[MarketplaceChannelProducts] (
                        [MarketplaceChannelProductID] int NOT NULL IDENTITY,
                        [MarketplaceChannelID] int NOT NULL,
                        [ProductID] int NOT NULL,
                        [IsIncluded] bit NOT NULL,
                        [RemoteProductID] nvarchar(100) NULL,
                        [LastSyncedAt] datetime2(0) NULL,
                        CONSTRAINT [PK_MarketplaceChannelProducts] PRIMARY KEY ([MarketplaceChannelProductID]),
                        CONSTRAINT [FK_MarketplaceChannelProducts_MarketplaceChannels] FOREIGN KEY ([MarketplaceChannelID]) REFERENCES [dbo].[MarketplaceChannels] ([MarketplaceChannelID]) ON DELETE CASCADE,
                        CONSTRAINT [FK_MarketplaceChannelProducts_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_MarketplaceChannelProducts_ProductID] ON [dbo].[MarketplaceChannelProducts] ([ProductID]);
                    CREATE UNIQUE INDEX [UQ_MarketplaceChannelProducts_Channel_Product] ON [dbo].[MarketplaceChannelProducts] ([MarketplaceChannelID], [ProductID]);
                END
                """);

            migrationBuilder.AlterColumn<string>(
                name: "SettingsJSON",
                table: "MarketplaceChannels",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "EmailAccounts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SettingsJSON",
                table: "WebsiteWhatsAppSettings",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "StorageSettingsJSON",
                table: "WebsiteStorageSettings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SettingsJSON",
                table: "WebsiteSmsSettings",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TurnstileSecretKey",
                table: "WebsiteCaptchaSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SettingsJSON",
                table: "PaymentGateways",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MerchantID",
                table: "PaymentGateways",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApiSecret",
                table: "PaymentGateways",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApiKey",
                table: "PaymentGateways",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SettingsJSON",
                table: "MarketplaceChannels",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "EmailAccounts",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
