using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public class FinancialLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialLedgerEntries",
                columns: table => new
                {
                    FinancialLedgerEntryID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Flow = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    AmountUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "char(3)", fixedLength: true, maxLength: 3, nullable: false),
                    OccurredDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OccurredTime = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Title = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    ReportToTax = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    VendorVisible = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    VendorID = table.Column<int>(type: "int", nullable: true),
                    OrderID = table.Column<int>(type: "int", nullable: true),
                    OrderItemID = table.Column<int>(type: "int", nullable: true),
                    PaymentID = table.Column<int>(type: "int", nullable: true),
                    SettlementID = table.Column<int>(type: "int", nullable: true),
                    WebsiteClientID = table.Column<int>(type: "int", nullable: true),
                    EventGroupId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    SupersedesEntryID = table.Column<long>(type: "bigint", nullable: true),
                    IsCurrent = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    ChangeNote = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedByMemberID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MetaJson = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialLedgerEntries", x => x.FinancialLedgerEntryID);
                    table.ForeignKey(
                        name: "FK_FinancialLedgerEntries_Currencies",
                        column: x => x.CurrencyCode,
                        principalTable: "Currencies",
                        principalColumn: "CurrencyCode");
                    table.ForeignKey(
                        name: "FK_FinancialLedgerEntries_Members",
                        column: x => x.CreatedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_FinancialLedgerEntries_OrderItems",
                        column: x => x.OrderItemID,
                        principalTable: "OrderItems",
                        principalColumn: "OrderItemID");
                    table.ForeignKey(
                        name: "FK_FinancialLedgerEntries_Orders",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
                    table.ForeignKey(
                        name: "FK_FinancialLedgerEntries_Payments",
                        column: x => x.PaymentID,
                        principalTable: "Payments",
                        principalColumn: "PaymentID");
                    table.ForeignKey(
                        name: "FK_FinancialLedgerEntries_Settlements",
                        column: x => x.SettlementID,
                        principalTable: "Settlements",
                        principalColumn: "SettlementID");
                    table.ForeignKey(
                        name: "FK_FinancialLedgerEntries_Supersedes",
                        column: x => x.SupersedesEntryID,
                        principalTable: "FinancialLedgerEntries",
                        principalColumn: "FinancialLedgerEntryID");
                    table.ForeignKey(
                        name: "FK_FinancialLedgerEntries_Vendors",
                        column: x => x.VendorID,
                        principalTable: "Vendors",
                        principalColumn: "VendorID");
                    table.ForeignKey(
                        name: "FK_FinancialLedgerEntries_WebsiteClients",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID");
                    table.ForeignKey(
                        name: "FK_FinancialLedgerEntries_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                });

            migrationBuilder.CreateIndex(name: "IX_FinancialLedgerEntries_CreatedByMemberID", table: "FinancialLedgerEntries", column: "CreatedByMemberID");
            migrationBuilder.CreateIndex(name: "IX_FinancialLedgerEntries_CurrencyCode", table: "FinancialLedgerEntries", column: "CurrencyCode");
            migrationBuilder.CreateIndex(name: "IX_FinancialLedgerEntries_EventGroup", table: "FinancialLedgerEntries", column: "EventGroupId");
            migrationBuilder.CreateIndex(name: "IX_FinancialLedgerEntries_OrderID", table: "FinancialLedgerEntries", column: "OrderID");
            migrationBuilder.CreateIndex(name: "IX_FinancialLedgerEntries_OrderItemID", table: "FinancialLedgerEntries", column: "OrderItemID");
            migrationBuilder.CreateIndex(name: "IX_FinancialLedgerEntries_PaymentID", table: "FinancialLedgerEntries", column: "PaymentID");
            migrationBuilder.CreateIndex(name: "IX_FinancialLedgerEntries_SettlementID", table: "FinancialLedgerEntries", column: "SettlementID");
            migrationBuilder.CreateIndex(name: "IX_FinancialLedgerEntries_Supersedes", table: "FinancialLedgerEntries", column: "SupersedesEntryID");
            migrationBuilder.CreateIndex(name: "IX_FinancialLedgerEntries_Type", table: "FinancialLedgerEntries", columns: new[] { "WebsiteID", "TransactionType", "IsCurrent" });
            migrationBuilder.CreateIndex(name: "IX_FinancialLedgerEntries_VendorID", table: "FinancialLedgerEntries", columns: new[] { "VendorID", "VendorVisible", "IsCurrent" });
            migrationBuilder.CreateIndex(name: "IX_FinancialLedgerEntries_Website_Date", table: "FinancialLedgerEntries", columns: new[] { "WebsiteID", "OccurredDate", "IsCurrent" });
            migrationBuilder.CreateIndex(name: "IX_FinancialLedgerEntries_WebsiteClientID", table: "FinancialLedgerEntries", column: "WebsiteClientID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "FinancialLedgerEntries");
        }
    }
}
