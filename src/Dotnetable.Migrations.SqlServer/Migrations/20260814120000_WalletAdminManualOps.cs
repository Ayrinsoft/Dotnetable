using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    [Migration("20260814120000_WalletAdminManualOps")]
    public class WalletAdminManualOps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.ClientWalletWithdrawals', N'Note') IS NULL
  ALTER TABLE [dbo].[ClientWalletWithdrawals] ADD [Note] NVARCHAR(500) NULL;
IF COL_LENGTH(N'dbo.ClientWalletWithdrawals', N'CreatedByMemberID') IS NULL
  ALTER TABLE [dbo].[ClientWalletWithdrawals] ADD [CreatedByMemberID] INT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ClientWalletWithdrawals_CreatedByMemberID' AND object_id = OBJECT_ID(N'dbo.ClientWalletWithdrawals'))
  CREATE NONCLUSTERED INDEX [IX_ClientWalletWithdrawals_CreatedByMemberID] ON [dbo].[ClientWalletWithdrawals]([CreatedByMemberID]);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ClientWalletWithdrawals_Members_CreatedBy')
  ALTER TABLE [dbo].[ClientWalletWithdrawals] ADD CONSTRAINT [FK_ClientWalletWithdrawals_Members_CreatedBy]
    FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members]([MemberID]);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ClientWalletWithdrawals_Members_CreatedBy')
  ALTER TABLE [dbo].[ClientWalletWithdrawals] DROP CONSTRAINT [FK_ClientWalletWithdrawals_Members_CreatedBy];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ClientWalletWithdrawals_CreatedByMemberID' AND object_id = OBJECT_ID(N'dbo.ClientWalletWithdrawals'))
  DROP INDEX [IX_ClientWalletWithdrawals_CreatedByMemberID] ON [dbo].[ClientWalletWithdrawals];
IF COL_LENGTH(N'dbo.ClientWalletWithdrawals', N'CreatedByMemberID') IS NOT NULL
  ALTER TABLE [dbo].[ClientWalletWithdrawals] DROP COLUMN [CreatedByMemberID];
IF COL_LENGTH(N'dbo.ClientWalletWithdrawals', N'Note') IS NOT NULL
  ALTER TABLE [dbo].[ClientWalletWithdrawals] DROP COLUMN [Note];
");
        }
    }
}
