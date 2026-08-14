using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    [Migration("20260814120020_WalletAdminManualOps")]
    public class WalletAdminManualOps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @db := DATABASE();
SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='ClientWalletWithdrawals' AND COLUMN_NAME='Note');
SET @s := IF(@e=0, 'ALTER TABLE ClientWalletWithdrawals ADD COLUMN Note VARCHAR(500) NULL, ADD COLUMN CreatedByMemberID INT NULL, ADD KEY IX_ClientWalletWithdrawals_CreatedByMemberID (CreatedByMemberID), ADD CONSTRAINT FK_ClientWalletWithdrawals_Members_CreatedBy FOREIGN KEY (CreatedByMemberID) REFERENCES Members (MemberID)', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @db := DATABASE();
SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='ClientWalletWithdrawals' AND COLUMN_NAME='Note');
SET @s := IF(@e>0, 'ALTER TABLE ClientWalletWithdrawals DROP FOREIGN KEY FK_ClientWalletWithdrawals_Members_CreatedBy, DROP INDEX IX_ClientWalletWithdrawals_CreatedByMemberID, DROP COLUMN CreatedByMemberID, DROP COLUMN Note', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;
");
        }
    }
}
