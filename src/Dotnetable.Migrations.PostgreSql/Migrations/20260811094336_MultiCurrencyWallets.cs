using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class MultiCurrencyWallets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_ClientWallets_WebsiteClientID",
                table: "ClientWallets");

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "ClientWalletWithdrawals",
                type: "character(3)",
                unicode: false,
                fixedLength: true,
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "ClientWallets",
                type: "character(3)",
                unicode: false,
                fixedLength: true,
                maxLength: 3,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "ClientWallets" cw
                SET "CurrencyCode" = LEFT(TRIM(w."DefaultCurrencyCode"), 3)
                FROM "Websites" w
                WHERE w."WebsiteID" = cw."WebsiteID"
                  AND (cw."CurrencyCode" IS NULL OR TRIM(cw."CurrencyCode") = '');
                """);

            migrationBuilder.Sql("""
                UPDATE "ClientWalletWithdrawals" cww
                SET "CurrencyCode" = LEFT(TRIM(COALESCE(NULLIF(TRIM(cw."CurrencyCode"), ''), w."DefaultCurrencyCode")), 3)
                FROM "ClientWallets" cw
                INNER JOIN "Websites" w ON w."WebsiteID" = cww."WebsiteID"
                WHERE cw."ClientWalletID" = cww."ClientWalletID"
                  AND (cww."CurrencyCode" IS NULL OR TRIM(cww."CurrencyCode") = '');
                """);

            migrationBuilder.Sql("""
                UPDATE "ClientWallets" SET "CurrencyCode" = (SELECT "CurrencyCode" FROM "Currencies" ORDER BY "CurrencyCode" LIMIT 1)
                WHERE "CurrencyCode" IS NULL OR TRIM("CurrencyCode") = '';
                UPDATE "ClientWalletWithdrawals" SET "CurrencyCode" = (SELECT "CurrencyCode" FROM "Currencies" ORDER BY "CurrencyCode" LIMIT 1)
                WHERE "CurrencyCode" IS NULL OR TRIM("CurrencyCode") = '';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "CurrencyCode",
                table: "ClientWallets",
                type: "character(3)",
                unicode: false,
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character(3)",
                oldUnicode: false,
                oldFixedLength: true,
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CurrencyCode",
                table: "ClientWalletWithdrawals",
                type: "character(3)",
                unicode: false,
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character(3)",
                oldUnicode: false,
                oldFixedLength: true,
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "WebsiteWalletCurrencies",
                columns: table => new
                {
                    WebsiteWalletCurrencyID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WebsiteID = table.Column<int>(type: "integer", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteWalletCurrencies", x => x.WebsiteWalletCurrencyID);
                    table.ForeignKey(
                        name: "FK_WebsiteWalletCurrencies_Currencies",
                        column: x => x.CurrencyCode,
                        principalTable: "Currencies",
                        principalColumn: "CurrencyCode");
                    table.ForeignKey(
                        name: "FK_WebsiteWalletCurrencies_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientWallets_CurrencyCode",
                table: "ClientWallets",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "UQ_ClientWallets_Client_Currency",
                table: "ClientWallets",
                columns: new[] { "WebsiteClientID", "CurrencyCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteWalletCurrencies_CurrencyCode",
                table: "WebsiteWalletCurrencies",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteWalletCurrencies_WebsiteID",
                table: "WebsiteWalletCurrencies",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "UQ_WebsiteWalletCurrencies_Website_Currency",
                table: "WebsiteWalletCurrencies",
                columns: new[] { "WebsiteID", "CurrencyCode" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO "WebsiteWalletCurrencies" ("WebsiteID", "CurrencyCode", "IsDefault", "IsActive", "CreatedAt")
                SELECT w."WebsiteID", LEFT(TRIM(w."DefaultCurrencyCode"), 3), TRUE, TRUE, NOW() AT TIME ZONE 'UTC'
                FROM "Websites" w
                WHERE w."DefaultCurrencyCode" IS NOT NULL AND TRIM(w."DefaultCurrencyCode") <> ''
                  AND EXISTS (SELECT 1 FROM "Currencies" c WHERE c."CurrencyCode" = LEFT(TRIM(w."DefaultCurrencyCode"), 3))
                  AND NOT EXISTS (
                      SELECT 1 FROM "WebsiteWalletCurrencies" x
                      WHERE x."WebsiteID" = w."WebsiteID" AND x."CurrencyCode" = LEFT(TRIM(w."DefaultCurrencyCode"), 3));
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientWallets_Currencies",
                table: "ClientWallets",
                column: "CurrencyCode",
                principalTable: "Currencies",
                principalColumn: "CurrencyCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientWallets_Currencies",
                table: "ClientWallets");

            migrationBuilder.DropTable(
                name: "WebsiteWalletCurrencies");

            migrationBuilder.DropIndex(
                name: "IX_ClientWallets_CurrencyCode",
                table: "ClientWallets");

            migrationBuilder.DropIndex(
                name: "UQ_ClientWallets_Client_Currency",
                table: "ClientWallets");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "ClientWalletWithdrawals");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "ClientWallets");

            migrationBuilder.CreateIndex(
                name: "UQ_ClientWallets_WebsiteClientID",
                table: "ClientWallets",
                column: "WebsiteClientID",
                unique: true);
        }
    }
}
