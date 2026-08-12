using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260812190000_ItemConditionAndPayrollBrackets")]
    public class ItemConditionAndPayrollBrackets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.StockDocumentLines', N'HealthGrade') IS NULL
BEGIN
  ALTER TABLE [dbo].[StockDocumentLines] ADD [HealthGrade] TINYINT NOT NULL
    CONSTRAINT [DF_StockDocumentLines_HealthGrade] DEFAULT ((0));
END

IF COL_LENGTH(N'dbo.VendorProducts', N'ItemCondition') IS NULL
BEGIN
  ALTER TABLE [dbo].[VendorProducts] ADD [ItemCondition] TINYINT NOT NULL
    CONSTRAINT [DF_VendorProducts_ItemCondition] DEFAULT ((1));
END
IF COL_LENGTH(N'dbo.VendorProducts', N'HealthGrade') IS NULL
BEGIN
  ALTER TABLE [dbo].[VendorProducts] ADD [HealthGrade] TINYINT NOT NULL
    CONSTRAINT [DF_VendorProducts_HealthGrade] DEFAULT ((0));
END

IF COL_LENGTH(N'dbo.EmployeeContracts', N'UseFlatRates') IS NULL
BEGIN
  ALTER TABLE [dbo].[EmployeeContracts] ADD [UseFlatRates] BIT NOT NULL
    CONSTRAINT [DF_EmployeeContracts_UseFlatRates] DEFAULT ((1));
END

IF OBJECT_ID(N'dbo.PayrollRateBrackets', N'U') IS NULL
BEGIN
  CREATE TABLE [dbo].[PayrollRateBrackets](
    [PayrollRateBracketID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [WebsiteID] INT NOT NULL,
    [Kind] TINYINT NOT NULL,
    [FromAmount] DECIMAL(18,4) NOT NULL,
    [ToAmount] DECIMAL(18,4) NULL,
    [Rate] DECIMAL(9,6) NOT NULL,
    [SortOrder] INT NOT NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_PayrollRateBrackets_IsActive] DEFAULT ((1)),
    [CreatedAt] DATETIME2(7) NOT NULL,
    CONSTRAINT [FK_PayrollRateBrackets_Websites] FOREIGN KEY([WebsiteID]) REFERENCES [dbo].[Websites]([WebsiteID])
  );
  CREATE NONCLUSTERED INDEX [IX_PayrollRateBrackets_Website_Kind]
    ON [dbo].[PayrollRateBrackets]([WebsiteID],[Kind],[SortOrder]);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.PayrollRateBrackets', N'U') IS NOT NULL DROP TABLE [dbo].[PayrollRateBrackets];
IF COL_LENGTH(N'dbo.EmployeeContracts', N'UseFlatRates') IS NOT NULL
BEGIN
  ALTER TABLE [dbo].[EmployeeContracts] DROP CONSTRAINT [DF_EmployeeContracts_UseFlatRates];
  ALTER TABLE [dbo].[EmployeeContracts] DROP COLUMN [UseFlatRates];
END
IF COL_LENGTH(N'dbo.VendorProducts', N'HealthGrade') IS NOT NULL
BEGIN
  ALTER TABLE [dbo].[VendorProducts] DROP CONSTRAINT [DF_VendorProducts_HealthGrade];
  ALTER TABLE [dbo].[VendorProducts] DROP COLUMN [HealthGrade];
END
IF COL_LENGTH(N'dbo.VendorProducts', N'ItemCondition') IS NOT NULL
BEGIN
  ALTER TABLE [dbo].[VendorProducts] DROP CONSTRAINT [DF_VendorProducts_ItemCondition];
  ALTER TABLE [dbo].[VendorProducts] DROP COLUMN [ItemCondition];
END
IF COL_LENGTH(N'dbo.StockDocumentLines', N'HealthGrade') IS NOT NULL
BEGIN
  ALTER TABLE [dbo].[StockDocumentLines] DROP CONSTRAINT [DF_StockDocumentLines_HealthGrade];
  ALTER TABLE [dbo].[StockDocumentLines] DROP COLUMN [HealthGrade];
END
");
        }
    }
}
