using System;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260812150000_GeneralLedger")]
    public class GeneralLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "ChartOfAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "ChartOfAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FiscalPeriods",
                columns: table => new
                {
                    FiscalPeriodID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PeriodFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodTo = table.Column<DateOnly>(type: "date", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedByMemberID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalPeriods", x => x.FiscalPeriodID);
                    table.ForeignKey(
                        name: "FK_FiscalPeriods_Members",
                        column: x => x.ClosedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_FiscalPeriods_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID",
                        onDelete: ReferentialAction.Cascade);
                });

            // SourceType was tinyint; convert to nvarchar for open categories.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[JournalEntries]') AND name = 'SourceType' AND system_type_id = TYPE_ID(N'tinyint'))
BEGIN
    ALTER TABLE [dbo].[JournalEntries] ALTER COLUMN [SourceType] NVARCHAR(40) NULL;
END
");

            migrationBuilder.AddColumn<string>(
                name: "SourceKey",
                table: "JournalEntries",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "JournalEntries",
                type: "char(3)",
                unicode: false,
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<bool>(
                name: "ReportToTax",
                table: "JournalEntries",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "FiscalPeriodID",
                table: "JournalEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PostedAt",
                table: "JournalEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PostedByMemberID",
                table: "JournalEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReversed",
                table: "JournalEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReversesJournalEntryID",
                table: "JournalEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByMemberID",
                table: "JournalEntries",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[JournalEntries]') AND name = 'CreatedAt' AND system_type_id = TYPE_ID(N'datetime'))
BEGIN
    ALTER TABLE [dbo].[JournalEntries] ALTER COLUMN [CreatedAt] DATETIME2 NOT NULL;
END
");

            migrationBuilder.CreateTable(
                name: "LedgerAccountMaps",
                columns: table => new
                {
                    LedgerAccountMapID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Flow = table.Column<byte>(type: "tinyint", nullable: true),
                    DebitAccountID = table.Column<int>(type: "int", nullable: false),
                    CreditAccountID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerAccountMaps", x => x.LedgerAccountMapID);
                    table.ForeignKey(
                        name: "FK_LedgerAccountMaps_Debit",
                        column: x => x.DebitAccountID,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "ChartOfAccountID");
                    table.ForeignKey(
                        name: "FK_LedgerAccountMaps_Credit",
                        column: x => x.CreditAccountID,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "ChartOfAccountID");
                    table.ForeignKey(
                        name: "FK_LedgerAccountMaps_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_ChartOfAccounts_Website_Code", table: "ChartOfAccounts", columns: new[] { "WebsiteID", "Code" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_FiscalPeriods_WebsiteID", table: "FiscalPeriods", columns: new[] { "WebsiteID", "PeriodFrom" });
            migrationBuilder.CreateIndex(name: "IX_FiscalPeriods_ClosedByMemberID", table: "FiscalPeriods", column: "ClosedByMemberID");
            migrationBuilder.CreateIndex(name: "IX_JournalEntries_EntryDate", table: "JournalEntries", columns: new[] { "WebsiteID", "EntryDate", "IsPosted" });
            migrationBuilder.CreateIndex(name: "IX_JournalEntries_FiscalPeriodID", table: "JournalEntries", column: "FiscalPeriodID");
            migrationBuilder.CreateIndex(name: "IX_JournalEntries_CurrencyCode", table: "JournalEntries", column: "CurrencyCode");
            migrationBuilder.CreateIndex(name: "IX_JournalEntries_PostedByMemberID", table: "JournalEntries", column: "PostedByMemberID");
            migrationBuilder.CreateIndex(name: "IX_JournalEntries_CreatedByMemberID", table: "JournalEntries", column: "CreatedByMemberID");
            migrationBuilder.CreateIndex(name: "IX_JournalEntries_ReversesJournalEntryID", table: "JournalEntries", column: "ReversesJournalEntryID");
            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_Website_SourceKey",
                table: "JournalEntries",
                columns: new[] { "WebsiteID", "SourceKey" },
                unique: true,
                filter: "[SourceKey] IS NOT NULL");
            migrationBuilder.CreateIndex(name: "IX_LedgerAccountMaps_Website_Type", table: "LedgerAccountMaps", columns: new[] { "WebsiteID", "TransactionType", "IsActive" });
            migrationBuilder.CreateIndex(name: "IX_LedgerAccountMaps_DebitAccountID", table: "LedgerAccountMaps", column: "DebitAccountID");
            migrationBuilder.CreateIndex(name: "IX_LedgerAccountMaps_CreditAccountID", table: "LedgerAccountMaps", column: "CreditAccountID");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_Currencies",
                table: "JournalEntries",
                column: "CurrencyCode",
                principalTable: "Currencies",
                principalColumn: "CurrencyCode");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_FiscalPeriods",
                table: "JournalEntries",
                column: "FiscalPeriodID",
                principalTable: "FiscalPeriods",
                principalColumn: "FiscalPeriodID");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_PostedBy",
                table: "JournalEntries",
                column: "PostedByMemberID",
                principalTable: "Members",
                principalColumn: "MemberID");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_CreatedBy",
                table: "JournalEntries",
                column: "CreatedByMemberID",
                principalTable: "Members",
                principalColumn: "MemberID");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_Reverses",
                table: "JournalEntries",
                column: "ReversesJournalEntryID",
                principalTable: "JournalEntries",
                principalColumn: "JournalEntryID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_JournalEntries_Currencies", table: "JournalEntries");
            migrationBuilder.DropForeignKey(name: "FK_JournalEntries_FiscalPeriods", table: "JournalEntries");
            migrationBuilder.DropForeignKey(name: "FK_JournalEntries_PostedBy", table: "JournalEntries");
            migrationBuilder.DropForeignKey(name: "FK_JournalEntries_CreatedBy", table: "JournalEntries");
            migrationBuilder.DropForeignKey(name: "FK_JournalEntries_Reverses", table: "JournalEntries");

            migrationBuilder.DropTable(name: "LedgerAccountMaps");
            migrationBuilder.DropTable(name: "FiscalPeriods");

            migrationBuilder.DropIndex(name: "IX_ChartOfAccounts_Website_Code", table: "ChartOfAccounts");
            migrationBuilder.DropIndex(name: "IX_JournalEntries_EntryDate", table: "JournalEntries");
            migrationBuilder.DropIndex(name: "IX_JournalEntries_FiscalPeriodID", table: "JournalEntries");
            migrationBuilder.DropIndex(name: "IX_JournalEntries_CurrencyCode", table: "JournalEntries");
            migrationBuilder.DropIndex(name: "IX_JournalEntries_PostedByMemberID", table: "JournalEntries");
            migrationBuilder.DropIndex(name: "IX_JournalEntries_CreatedByMemberID", table: "JournalEntries");
            migrationBuilder.DropIndex(name: "IX_JournalEntries_ReversesJournalEntryID", table: "JournalEntries");
            migrationBuilder.DropIndex(name: "IX_JournalEntries_Website_SourceKey", table: "JournalEntries");

            migrationBuilder.DropColumn(name: "IsSystem", table: "ChartOfAccounts");
            migrationBuilder.DropColumn(name: "SortOrder", table: "ChartOfAccounts");
            migrationBuilder.DropColumn(name: "SourceKey", table: "JournalEntries");
            migrationBuilder.DropColumn(name: "CurrencyCode", table: "JournalEntries");
            migrationBuilder.DropColumn(name: "ReportToTax", table: "JournalEntries");
            migrationBuilder.DropColumn(name: "FiscalPeriodID", table: "JournalEntries");
            migrationBuilder.DropColumn(name: "PostedAt", table: "JournalEntries");
            migrationBuilder.DropColumn(name: "PostedByMemberID", table: "JournalEntries");
            migrationBuilder.DropColumn(name: "IsReversed", table: "JournalEntries");
            migrationBuilder.DropColumn(name: "ReversesJournalEntryID", table: "JournalEntries");
            migrationBuilder.DropColumn(name: "CreatedByMemberID", table: "JournalEntries");
        }
    }
}
