using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260812210000_FiscalCalendarOpeningBalance")]
    public class FiscalCalendarOpeningBalance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Websites', N'FiscalPeriodCadence') IS NULL
  ALTER TABLE [dbo].[Websites] ADD [FiscalPeriodCadence] TINYINT NOT NULL
    CONSTRAINT [DF_Websites_FiscalPeriodCadence] DEFAULT ((3));
IF COL_LENGTH(N'dbo.Websites', N'FiscalYearStartMonth') IS NULL
  ALTER TABLE [dbo].[Websites] ADD [FiscalYearStartMonth] TINYINT NOT NULL
    CONSTRAINT [DF_Websites_FiscalYearStartMonth] DEFAULT ((1));
IF COL_LENGTH(N'dbo.Websites', N'FiscalWeekStartDay') IS NULL
  ALTER TABLE [dbo].[Websites] ADD [FiscalWeekStartDay] TINYINT NOT NULL
    CONSTRAINT [DF_Websites_FiscalWeekStartDay] DEFAULT ((1));
IF COL_LENGTH(N'dbo.Websites', N'FiscalCloseDueDays') IS NULL
  ALTER TABLE [dbo].[Websites] ADD [FiscalCloseDueDays] INT NOT NULL
    CONSTRAINT [DF_Websites_FiscalCloseDueDays] DEFAULT ((5));

IF COL_LENGTH(N'dbo.FiscalPeriods', N'CloseDueDate') IS NULL
  ALTER TABLE [dbo].[FiscalPeriods] ADD [CloseDueDate] DATE NULL;
IF COL_LENGTH(N'dbo.FiscalPeriods', N'OpeningJournalEntryID') IS NULL
  ALTER TABLE [dbo].[FiscalPeriods] ADD [OpeningJournalEntryID] INT NULL;
IF COL_LENGTH(N'dbo.FiscalPeriods', N'ClosingJournalEntryID') IS NULL
  ALTER TABLE [dbo].[FiscalPeriods] ADD [ClosingJournalEntryID] INT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_FiscalPeriods_Website_Closed' AND object_id = OBJECT_ID(N'dbo.FiscalPeriods'))
  CREATE NONCLUSTERED INDEX [IX_FiscalPeriods_Website_Closed] ON [dbo].[FiscalPeriods]([WebsiteID],[IsClosed],[PeriodFrom]);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_FiscalPeriods_Website_Closed' AND object_id = OBJECT_ID(N'dbo.FiscalPeriods'))
  DROP INDEX [IX_FiscalPeriods_Website_Closed] ON [dbo].[FiscalPeriods];
IF COL_LENGTH(N'dbo.FiscalPeriods', N'ClosingJournalEntryID') IS NOT NULL
  ALTER TABLE [dbo].[FiscalPeriods] DROP COLUMN [ClosingJournalEntryID];
IF COL_LENGTH(N'dbo.FiscalPeriods', N'OpeningJournalEntryID') IS NOT NULL
  ALTER TABLE [dbo].[FiscalPeriods] DROP COLUMN [OpeningJournalEntryID];
IF COL_LENGTH(N'dbo.FiscalPeriods', N'CloseDueDate') IS NOT NULL
  ALTER TABLE [dbo].[FiscalPeriods] DROP COLUMN [CloseDueDate];
IF COL_LENGTH(N'dbo.Websites', N'FiscalCloseDueDays') IS NOT NULL
BEGIN ALTER TABLE [dbo].[Websites] DROP CONSTRAINT [DF_Websites_FiscalCloseDueDays]; ALTER TABLE [dbo].[Websites] DROP COLUMN [FiscalCloseDueDays]; END
IF COL_LENGTH(N'dbo.Websites', N'FiscalWeekStartDay') IS NOT NULL
BEGIN ALTER TABLE [dbo].[Websites] DROP CONSTRAINT [DF_Websites_FiscalWeekStartDay]; ALTER TABLE [dbo].[Websites] DROP COLUMN [FiscalWeekStartDay]; END
IF COL_LENGTH(N'dbo.Websites', N'FiscalYearStartMonth') IS NOT NULL
BEGIN ALTER TABLE [dbo].[Websites] DROP CONSTRAINT [DF_Websites_FiscalYearStartMonth]; ALTER TABLE [dbo].[Websites] DROP COLUMN [FiscalYearStartMonth]; END
IF COL_LENGTH(N'dbo.Websites', N'FiscalPeriodCadence') IS NOT NULL
BEGIN ALTER TABLE [dbo].[Websites] DROP CONSTRAINT [DF_Websites_FiscalPeriodCadence]; ALTER TABLE [dbo].[Websites] DROP COLUMN [FiscalPeriodCadence]; END
");
        }
    }
}
