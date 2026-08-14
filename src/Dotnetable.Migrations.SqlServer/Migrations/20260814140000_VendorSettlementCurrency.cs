using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    [Migration("20260814140000_VendorSettlementCurrency")]
    public class VendorSettlementCurrency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Vendors', N'SettlementCurrencyCode') IS NULL
  ALTER TABLE [dbo].[Vendors] ADD [SettlementCurrencyCode] CHAR(3) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Vendors_SettlementCurrencyCode' AND object_id = OBJECT_ID(N'dbo.Vendors'))
  CREATE NONCLUSTERED INDEX [IX_Vendors_SettlementCurrencyCode] ON [dbo].[Vendors]([SettlementCurrencyCode]);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Vendors_Currencies_Settlement')
  ALTER TABLE [dbo].[Vendors] ADD CONSTRAINT [FK_Vendors_Currencies_Settlement]
    FOREIGN KEY ([SettlementCurrencyCode]) REFERENCES [dbo].[Currencies]([CurrencyCode]);

IF COL_LENGTH(N'dbo.Settlements', N'SourceCurrencyCode') IS NULL
  ALTER TABLE [dbo].[Settlements] ADD
    [SourceCurrencyCode] CHAR(3) NULL,
    [SourceNetAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_Settlements_SourceNet] DEFAULT (0),
    [SourceTaxAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_Settlements_SourceTax] DEFAULT (0),
    [SourceTotalAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_Settlements_SourceTotal] DEFAULT (0),
    [BridgeUsdAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_Settlements_BridgeUsd] DEFAULT (0),
    [ExchangeRateToUsd] DECIMAL(18,6) NULL,
    [ExchangeRateUsdToSettle] DECIMAL(18,6) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Settlements_SourceCurrencyCode' AND object_id = OBJECT_ID(N'dbo.Settlements'))
  CREATE NONCLUSTERED INDEX [IX_Settlements_SourceCurrencyCode] ON [dbo].[Settlements]([SourceCurrencyCode]);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Settlements_Currencies_Source')
  ALTER TABLE [dbo].[Settlements] ADD CONSTRAINT [FK_Settlements_Currencies_Source]
    FOREIGN KEY ([SourceCurrencyCode]) REFERENCES [dbo].[Currencies]([CurrencyCode]);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Settlements_Currencies_Source')
  ALTER TABLE [dbo].[Settlements] DROP CONSTRAINT [FK_Settlements_Currencies_Source];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Settlements_SourceCurrencyCode' AND object_id = OBJECT_ID(N'dbo.Settlements'))
  DROP INDEX [IX_Settlements_SourceCurrencyCode] ON [dbo].[Settlements];
IF COL_LENGTH(N'dbo.Settlements', N'SourceCurrencyCode') IS NOT NULL
  ALTER TABLE [dbo].[Settlements] DROP COLUMN [SourceCurrencyCode], [SourceNetAmount], [SourceTaxAmount], [SourceTotalAmount], [BridgeUsdAmount], [ExchangeRateToUsd], [ExchangeRateUsdToSettle];

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Vendors_Currencies_Settlement')
  ALTER TABLE [dbo].[Vendors] DROP CONSTRAINT [FK_Vendors_Currencies_Settlement];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Vendors_SettlementCurrencyCode' AND object_id = OBJECT_ID(N'dbo.Vendors'))
  DROP INDEX [IX_Vendors_SettlementCurrencyCode] ON [dbo].[Vendors];
IF COL_LENGTH(N'dbo.Vendors', N'SettlementCurrencyCode') IS NOT NULL
  ALTER TABLE [dbo].[Vendors] DROP COLUMN [SettlementCurrencyCode];
");
        }
    }
}
