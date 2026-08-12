using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260812180000_ReturnStockAndQc")]
    public class ReturnStockAndQc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.StockDocuments', N'PaymentRefundID') IS NULL
BEGIN
  ALTER TABLE [dbo].[StockDocuments] ADD [PaymentRefundID] INT NULL;
  ALTER TABLE [dbo].[StockDocuments] WITH CHECK ADD CONSTRAINT [FK_StockDocuments_PaymentRefunds]
    FOREIGN KEY([PaymentRefundID]) REFERENCES [dbo].[PaymentRefunds]([PaymentRefundID]);
END
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StockDocuments_PaymentRefundID' AND object_id = OBJECT_ID(N'dbo.StockDocuments'))
  CREATE NONCLUSTERED INDEX [IX_StockDocuments_PaymentRefundID] ON [dbo].[StockDocuments]([PaymentRefundID]);

IF COL_LENGTH(N'dbo.StockDocumentLines', N'ReturnCondition') IS NULL
BEGIN
  ALTER TABLE [dbo].[StockDocumentLines] ADD [ReturnCondition] TINYINT NOT NULL
    CONSTRAINT [DF_StockDocumentLines_ReturnCondition] DEFAULT ((0));
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StockDocuments_PaymentRefundID' AND object_id = OBJECT_ID(N'dbo.StockDocuments'))
  DROP INDEX [IX_StockDocuments_PaymentRefundID] ON [dbo].[StockDocuments];
IF OBJECT_ID(N'dbo.FK_StockDocuments_PaymentRefunds', N'F') IS NOT NULL
  ALTER TABLE [dbo].[StockDocuments] DROP CONSTRAINT [FK_StockDocuments_PaymentRefunds];
IF COL_LENGTH(N'dbo.StockDocuments', N'PaymentRefundID') IS NOT NULL
  ALTER TABLE [dbo].[StockDocuments] DROP COLUMN [PaymentRefundID];
IF OBJECT_ID(N'dbo.DF_StockDocumentLines_ReturnCondition', N'D') IS NOT NULL
  ALTER TABLE [dbo].[StockDocumentLines] DROP CONSTRAINT [DF_StockDocumentLines_ReturnCondition];
IF COL_LENGTH(N'dbo.StockDocumentLines', N'ReturnCondition') IS NOT NULL
  ALTER TABLE [dbo].[StockDocumentLines] DROP COLUMN [ReturnCondition];
");
        }
    }
}
