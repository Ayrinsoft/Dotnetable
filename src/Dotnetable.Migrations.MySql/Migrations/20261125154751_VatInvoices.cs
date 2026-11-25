using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class VatInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InputTaxSnapshot",
                table: "TaxPeriods",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "VatInvoices",
                columns: table => new
                {
                    VatInvoiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    OrderID = table.Column<int>(type: "int", nullable: true),
                    CurrencyCode = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    OurName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    OurNationalId = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true),
                    OurEconomicCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    OurVatNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    OurRegistrationNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    OurPhone = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true),
                    OurEmail = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    OurAddress = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    OtherName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    OtherNationalId = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true),
                    OtherEconomicCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    OtherPhone = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true),
                    OtherEmail = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    OtherAddress = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    OtherPostalCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    NetTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PayableTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PricesIncludeTax = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Note = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    CreatedByMemberID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VatInvoices", x => x.VatInvoiceID);
                    table.ForeignKey(
                        name: "FK_VatInvoices_Members_CreatedByMemberID",
                        column: x => x.CreatedByMemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID");
                    table.ForeignKey(
                        name: "FK_VatInvoices_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
                    table.ForeignKey(
                        name: "FK_VatInvoices_Websites_WebsiteID",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VatInvoiceLines",
                columns: table => new
                {
                    VatInvoiceLineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    VatInvoiceID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Unit = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PayableAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VatInvoiceLines", x => x.VatInvoiceLineID);
                    table.ForeignKey(
                        name: "FK_VatInvoiceLines_VatInvoices_VatInvoiceID",
                        column: x => x.VatInvoiceID,
                        principalTable: "VatInvoices",
                        principalColumn: "VatInvoiceID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_VatInvoiceLines_Invoice",
                table: "VatInvoiceLines",
                column: "VatInvoiceID");

            migrationBuilder.CreateIndex(
                name: "IX_VatInvoices_CreatedByMemberID",
                table: "VatInvoices",
                column: "CreatedByMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_VatInvoices_Number",
                table: "VatInvoices",
                columns: new[] { "WebsiteID", "Direction", "InvoiceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VatInvoices_OrderID",
                table: "VatInvoices",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_VatInvoices_Website_Date",
                table: "VatInvoices",
                columns: new[] { "WebsiteID", "InvoiceDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VatInvoiceLines");

            migrationBuilder.DropTable(
                name: "VatInvoices");

            migrationBuilder.DropColumn(
                name: "InputTaxSnapshot",
                table: "TaxPeriods");
        }
    }
}
