using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    [Migration("20260814120010_WalletAdminManualOps")]
    public class WalletAdminManualOps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE ""ClientWalletWithdrawals"" ADD COLUMN IF NOT EXISTS ""Note"" VARCHAR(500) NULL;
ALTER TABLE ""ClientWalletWithdrawals"" ADD COLUMN IF NOT EXISTS ""CreatedByMemberID"" INT NULL;
CREATE INDEX IF NOT EXISTS ""IX_ClientWalletWithdrawals_CreatedByMemberID"" ON ""ClientWalletWithdrawals"" (""CreatedByMemberID"");
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint WHERE conname = 'FK_ClientWalletWithdrawals_Members_CreatedBy') THEN
    ALTER TABLE ""ClientWalletWithdrawals""
      ADD CONSTRAINT ""FK_ClientWalletWithdrawals_Members_CreatedBy""
      FOREIGN KEY (""CreatedByMemberID"") REFERENCES ""Members"" (""MemberID"");
  END IF;
END $$;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE ""ClientWalletWithdrawals"" DROP CONSTRAINT IF EXISTS ""FK_ClientWalletWithdrawals_Members_CreatedBy"";
DROP INDEX IF EXISTS ""IX_ClientWalletWithdrawals_CreatedByMemberID"";
ALTER TABLE ""ClientWalletWithdrawals"" DROP COLUMN IF EXISTS ""CreatedByMemberID"";
ALTER TABLE ""ClientWalletWithdrawals"" DROP COLUMN IF EXISTS ""Note"";
");
        }
    }
}
