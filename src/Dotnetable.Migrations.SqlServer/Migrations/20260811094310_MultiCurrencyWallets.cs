using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
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

            // Add as nullable first, backfill from website default currency, then enforce NOT NULL + FK.
            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "ClientWallets",
                type: "char(3)",
                unicode: false,
                fixedLength: true,
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "ClientWalletWithdrawals",
                type: "char(3)",
                unicode: false,
                fixedLength: true,
                maxLength: 3,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE cw
                SET CurrencyCode = LEFT(LTRIM(RTRIM(w.DefaultCurrencyCode)), 3)
                FROM ClientWallets cw
                INNER JOIN Websites w ON w.WebsiteID = cw.WebsiteID
                WHERE cw.CurrencyCode IS NULL OR LTRIM(RTRIM(cw.CurrencyCode)) = '';
                """);

            migrationBuilder.Sql("""
                UPDATE cww
                SET CurrencyCode = LEFT(LTRIM(RTRIM(COALESCE(NULLIF(LTRIM(RTRIM(cw.CurrencyCode)), ''), w.DefaultCurrencyCode))), 3)
                FROM ClientWalletWithdrawals cww
                INNER JOIN ClientWallets cw ON cw.ClientWalletID = cww.ClientWalletID
                INNER JOIN Websites w ON w.WebsiteID = cww.WebsiteID
                WHERE cww.CurrencyCode IS NULL OR LTRIM(RTRIM(cww.CurrencyCode)) = '';
                """);

            // Fallback if any still empty (should not happen): use USD if present else first currency.
            migrationBuilder.Sql("""
                DECLARE @fb CHAR(3) = (SELECT TOP 1 CurrencyCode FROM Currencies WHERE CurrencyCode = 'USD');
                IF @fb IS NULL SET @fb = (SELECT TOP 1 CurrencyCode FROM Currencies ORDER BY CurrencyCode);
                IF @fb IS NOT NULL
                BEGIN
                    UPDATE ClientWallets SET CurrencyCode = @fb WHERE CurrencyCode IS NULL OR LTRIM(RTRIM(CurrencyCode)) = '';
                    UPDATE ClientWalletWithdrawals SET CurrencyCode = @fb WHERE CurrencyCode IS NULL OR LTRIM(RTRIM(CurrencyCode)) = '';
                END
                """);

            migrationBuilder.AlterColumn<string>(
                name: "CurrencyCode",
                table: "ClientWallets",
                type: "char(3)",
                unicode: false,
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(3)",
                oldUnicode: false,
                oldFixedLength: true,
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CurrencyCode",
                table: "ClientWalletWithdrawals",
                type: "char(3)",
                unicode: false,
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(3)",
                oldUnicode: false,
                oldFixedLength: true,
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "WebsiteWalletCurrencies",
                columns: table => new
                {
                    WebsiteWalletCurrencyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WebsiteID = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
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

            migrationBuilder.Sql("""
                INSERT INTO WebsiteWalletCurrencies (WebsiteID, CurrencyCode, IsDefault, IsActive, CreatedAt)
                SELECT w.WebsiteID, LEFT(LTRIM(RTRIM(w.DefaultCurrencyCode)), 3), 1, 1, SYSUTCDATETIME()
                FROM Websites w
                WHERE w.DefaultCurrencyCode IS NOT NULL
                  AND LTRIM(RTRIM(w.DefaultCurrencyCode)) <> ''
                  AND EXISTS (SELECT 1 FROM Currencies c WHERE c.CurrencyCode = LEFT(LTRIM(RTRIM(w.DefaultCurrencyCode)), 3))
                  AND NOT EXISTS (
                      SELECT 1 FROM WebsiteWalletCurrencies x
                      WHERE x.WebsiteID = w.WebsiteID AND x.CurrencyCode = LEFT(LTRIM(RTRIM(w.DefaultCurrencyCode)), 3));
                """);

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

            // (Backfill already done above before NOT NULL.)

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
