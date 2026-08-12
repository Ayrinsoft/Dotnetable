using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260812200000_CountVarianceTaxPeriod")]
    public class CountVarianceTaxPeriod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.StockDocumentLines', N'BookQuantity') IS NULL
  ALTER TABLE [dbo].[StockDocumentLines] ADD [BookQuantity] INT NOT NULL
    CONSTRAINT [DF_StockDocumentLines_BookQuantity] DEFAULT ((0));
IF COL_LENGTH(N'dbo.StockDocumentLines', N'CountedQuantity') IS NULL
  ALTER TABLE [dbo].[StockDocumentLines] ADD [CountedQuantity] INT NULL;

IF OBJECT_ID(N'dbo.TaxPeriods', N'U') IS NULL
BEGIN
  CREATE TABLE [dbo].[TaxPeriods](
    [TaxPeriodID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [WebsiteID] INT NOT NULL,
    [PeriodCode] NVARCHAR(30) NOT NULL,
    [FromDate] DATE NOT NULL,
    [ToDate] DATE NOT NULL,
    [Status] TINYINT NOT NULL,
    [OutputTaxSnapshot] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_TaxPeriods_OutputTax] DEFAULT ((0)),
    [SettlementTaxSnapshot] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_TaxPeriods_SettlementTax] DEFAULT ((0)),
    [NetTaxSnapshot] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_TaxPeriods_NetTax] DEFAULT ((0)),
    [OutputOrderCount] INT NOT NULL CONSTRAINT [DF_TaxPeriods_OrderCount] DEFAULT ((0)),
    [SettlementCount] INT NOT NULL CONSTRAINT [DF_TaxPeriods_SettlementCount] DEFAULT ((0)),
    [Note] NVARCHAR(1000) NULL,
    [CreatedByMemberID] INT NULL,
    [ClosedByMemberID] INT NULL,
    [ClosedAt] DATETIME2(7) NULL,
    [SnapshotAt] DATETIME2(7) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL,
    CONSTRAINT [FK_TaxPeriods_Websites] FOREIGN KEY([WebsiteID]) REFERENCES [dbo].[Websites]([WebsiteID]),
    CONSTRAINT [FK_TaxPeriods_CreatedBy] FOREIGN KEY([CreatedByMemberID]) REFERENCES [dbo].[Members]([MemberID]),
    CONSTRAINT [FK_TaxPeriods_ClosedBy] FOREIGN KEY([ClosedByMemberID]) REFERENCES [dbo].[Members]([MemberID])
  );
  CREATE UNIQUE NONCLUSTERED INDEX [IX_TaxPeriods_Website_Code] ON [dbo].[TaxPeriods]([WebsiteID],[PeriodCode]);
  CREATE NONCLUSTERED INDEX [IX_TaxPeriods_Website_Status] ON [dbo].[TaxPeriods]([WebsiteID],[Status],[FromDate]);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.TaxPeriods', N'U') IS NOT NULL DROP TABLE [dbo].[TaxPeriods];
IF COL_LENGTH(N'dbo.StockDocumentLines', N'CountedQuantity') IS NOT NULL
  ALTER TABLE [dbo].[StockDocumentLines] DROP COLUMN [CountedQuantity];
IF COL_LENGTH(N'dbo.StockDocumentLines', N'BookQuantity') IS NOT NULL
BEGIN
  ALTER TABLE [dbo].[StockDocumentLines] DROP CONSTRAINT [DF_StockDocumentLines_BookQuantity];
  ALTER TABLE [dbo].[StockDocumentLines] DROP COLUMN [BookQuantity];
END
");
        }
    }
}
