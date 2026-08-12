using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    [Migration("20260812210010_FiscalCalendarOpeningBalance")]
    public class FiscalCalendarOpeningBalance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE ""Websites"" ADD COLUMN IF NOT EXISTS ""FiscalPeriodCadence"" SMALLINT NOT NULL DEFAULT 3;
ALTER TABLE ""Websites"" ADD COLUMN IF NOT EXISTS ""FiscalYearStartMonth"" SMALLINT NOT NULL DEFAULT 1;
ALTER TABLE ""Websites"" ADD COLUMN IF NOT EXISTS ""FiscalWeekStartDay"" SMALLINT NOT NULL DEFAULT 1;
ALTER TABLE ""Websites"" ADD COLUMN IF NOT EXISTS ""FiscalCloseDueDays"" INT NOT NULL DEFAULT 5;
ALTER TABLE ""FiscalPeriods"" ADD COLUMN IF NOT EXISTS ""CloseDueDate"" DATE NULL;
ALTER TABLE ""FiscalPeriods"" ADD COLUMN IF NOT EXISTS ""OpeningJournalEntryID"" INT NULL;
ALTER TABLE ""FiscalPeriods"" ADD COLUMN IF NOT EXISTS ""ClosingJournalEntryID"" INT NULL;
CREATE INDEX IF NOT EXISTS ""IX_FiscalPeriods_Website_Closed"" ON ""FiscalPeriods"" (""WebsiteID"", ""IsClosed"", ""PeriodFrom"");
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP INDEX IF EXISTS ""IX_FiscalPeriods_Website_Closed"";
ALTER TABLE ""FiscalPeriods"" DROP COLUMN IF EXISTS ""ClosingJournalEntryID"";
ALTER TABLE ""FiscalPeriods"" DROP COLUMN IF EXISTS ""OpeningJournalEntryID"";
ALTER TABLE ""FiscalPeriods"" DROP COLUMN IF EXISTS ""CloseDueDate"";
ALTER TABLE ""Websites"" DROP COLUMN IF EXISTS ""FiscalCloseDueDays"";
ALTER TABLE ""Websites"" DROP COLUMN IF EXISTS ""FiscalWeekStartDay"";
ALTER TABLE ""Websites"" DROP COLUMN IF EXISTS ""FiscalYearStartMonth"";
ALTER TABLE ""Websites"" DROP COLUMN IF EXISTS ""FiscalPeriodCadence"";
");
        }
    }
}
