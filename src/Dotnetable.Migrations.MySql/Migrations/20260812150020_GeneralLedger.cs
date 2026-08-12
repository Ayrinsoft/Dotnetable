using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    public class GeneralLedger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(name: "IsSystem", table: "ChartOfAccounts", type: "tinyint(1)", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<int>(name: "SortOrder", table: "ChartOfAccounts", type: "int", nullable: false, defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FiscalPeriods",
                columns: table => new
                {
                    FiscalPeriodID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    PeriodFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodTo = table.Column<DateOnly>(type: "date", nullable: false),
                    IsClosed = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ClosedByMemberID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalPeriods", x => x.FiscalPeriodID);
                    table.ForeignKey(name: "FK_FiscalPeriods_Members", column: x => x.ClosedByMemberID, principalTable: "Members", principalColumn: "MemberID");
                    table.ForeignKey(name: "FK_FiscalPeriods_Websites", column: x => x.WebsiteID, principalTable: "Websites", principalColumn: "WebsiteID");
                });

            migrationBuilder.Sql("ALTER TABLE `JournalEntries` MODIFY COLUMN `SourceType` varchar(40) NULL;");

            migrationBuilder.AddColumn<string>(name: "SourceKey", table: "JournalEntries", type: "varchar(80)", maxLength: 80, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CurrencyCode", table: "JournalEntries", type: "char(3)", maxLength: 3, nullable: false, defaultValue: "USD");
            migrationBuilder.AddColumn<bool>(name: "ReportToTax", table: "JournalEntries", type: "tinyint(1)", nullable: false, defaultValue: true);
            migrationBuilder.AddColumn<int>(name: "FiscalPeriodID", table: "JournalEntries", type: "int", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "PostedAt", table: "JournalEntries", type: "datetime(6)", nullable: true);
            migrationBuilder.AddColumn<int>(name: "PostedByMemberID", table: "JournalEntries", type: "int", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "IsReversed", table: "JournalEntries", type: "tinyint(1)", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<int>(name: "ReversesJournalEntryID", table: "JournalEntries", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "CreatedByMemberID", table: "JournalEntries", type: "int", nullable: true);

            migrationBuilder.CreateTable(
                name: "LedgerAccountMaps",
                columns: table => new
                {
                    LedgerAccountMapID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Flow = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    DebitAccountID = table.Column<int>(type: "int", nullable: false),
                    CreditAccountID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerAccountMaps", x => x.LedgerAccountMapID);
                    table.ForeignKey(name: "FK_LedgerAccountMaps_Debit", column: x => x.DebitAccountID, principalTable: "ChartOfAccounts", principalColumn: "ChartOfAccountID");
                    table.ForeignKey(name: "FK_LedgerAccountMaps_Credit", column: x => x.CreditAccountID, principalTable: "ChartOfAccounts", principalColumn: "ChartOfAccountID");
                    table.ForeignKey(name: "FK_LedgerAccountMaps_Websites", column: x => x.WebsiteID, principalTable: "Websites", principalColumn: "WebsiteID");
                });

            migrationBuilder.CreateIndex(name: "IX_ChartOfAccounts_Website_Code", table: "ChartOfAccounts", columns: new[] { "WebsiteID", "Code" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_FiscalPeriods_WebsiteID", table: "FiscalPeriods", columns: new[] { "WebsiteID", "PeriodFrom" });
            migrationBuilder.CreateIndex(name: "IX_JournalEntries_FiscalPeriodID", table: "JournalEntries", column: "FiscalPeriodID");
            migrationBuilder.CreateIndex(name: "IX_JournalEntries_CurrencyCode", table: "JournalEntries", column: "CurrencyCode");
            migrationBuilder.CreateIndex(name: "IX_LedgerAccountMaps_Website_Type", table: "LedgerAccountMaps", columns: new[] { "WebsiteID", "TransactionType", "IsActive" });

            migrationBuilder.AddForeignKey(name: "FK_JournalEntries_Currencies", table: "JournalEntries", column: "CurrencyCode", principalTable: "Currencies", principalColumn: "CurrencyCode");
            migrationBuilder.AddForeignKey(name: "FK_JournalEntries_FiscalPeriods", table: "JournalEntries", column: "FiscalPeriodID", principalTable: "FiscalPeriods", principalColumn: "FiscalPeriodID");
            migrationBuilder.AddForeignKey(name: "FK_JournalEntries_PostedBy", table: "JournalEntries", column: "PostedByMemberID", principalTable: "Members", principalColumn: "MemberID");
            migrationBuilder.AddForeignKey(name: "FK_JournalEntries_CreatedBy", table: "JournalEntries", column: "CreatedByMemberID", principalTable: "Members", principalColumn: "MemberID");
            migrationBuilder.AddForeignKey(name: "FK_JournalEntries_Reverses", table: "JournalEntries", column: "ReversesJournalEntryID", principalTable: "JournalEntries", principalColumn: "JournalEntryID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_JournalEntries_Currencies", table: "JournalEntries");
            migrationBuilder.DropForeignKey(name: "FK_JournalEntries_FiscalPeriods", table: "JournalEntries");
            migrationBuilder.DropForeignKey(name: "FK_JournalEntries_PostedBy", table: "JournalEntries");
            migrationBuilder.DropForeignKey(name: "FK_JournalEntries_CreatedBy", table: "JournalEntries");
            migrationBuilder.DropForeignKey(name: "FK_JournalEntries_Reverses", table: "JournalEntries");
            migrationBuilder.DropTable(name: "LedgerAccountMaps");
            migrationBuilder.DropTable(name: "FiscalPeriods");
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
