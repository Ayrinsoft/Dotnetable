using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    [Migration("20260812210020_FiscalCalendarOpeningBalance")]
    public class FiscalCalendarOpeningBalance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @db := DATABASE();
SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='Websites' AND COLUMN_NAME='FiscalPeriodCadence');
SET @s := IF(@e=0, 'ALTER TABLE Websites ADD COLUMN FiscalPeriodCadence TINYINT NOT NULL DEFAULT 3, ADD COLUMN FiscalYearStartMonth TINYINT NOT NULL DEFAULT 1, ADD COLUMN FiscalWeekStartDay TINYINT NOT NULL DEFAULT 1, ADD COLUMN FiscalCloseDueDays INT NOT NULL DEFAULT 5', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;

SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='FiscalPeriods' AND COLUMN_NAME='CloseDueDate');
SET @s := IF(@e=0, 'ALTER TABLE FiscalPeriods ADD COLUMN CloseDueDate DATE NULL, ADD COLUMN OpeningJournalEntryID INT NULL, ADD COLUMN ClosingJournalEntryID INT NULL, ADD KEY IX_FiscalPeriods_Website_Closed (WebsiteID, IsClosed, PeriodFrom)', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @db := DATABASE();
SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='FiscalPeriods' AND COLUMN_NAME='CloseDueDate');
SET @s := IF(@e>0, 'ALTER TABLE FiscalPeriods DROP COLUMN CloseDueDate, DROP COLUMN OpeningJournalEntryID, DROP COLUMN ClosingJournalEntryID', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;
SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='Websites' AND COLUMN_NAME='FiscalPeriodCadence');
SET @s := IF(@e>0, 'ALTER TABLE Websites DROP COLUMN FiscalPeriodCadence, DROP COLUMN FiscalYearStartMonth, DROP COLUMN FiscalWeekStartDay, DROP COLUMN FiscalCloseDueDays', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;
");
        }
    }
}
