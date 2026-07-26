using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class SiteCurrencyPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StorePricesInUsd",
                table: "Websites",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "When true, also persist USD dual columns; default site currency is always operational authority.");

            migrationBuilder.AddColumn<decimal>(
                name: "OverridePriceLocal",
                table: "VendorProducts",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReferencePrice",
                table: "VendorProducts",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CompareAtPrice",
                table: "ProductVariants",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReferencePrice",
                table: "ProductVariants",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CompareAtPrice",
                table: "ProductVariantPriceHistories",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReferencePrice",
                table: "ProductVariantPriceHistories",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            // Backfill site-currency prices from USD × website default rate (1 when no rate).
            migrationBuilder.Sql("""
                UPDATE v SET
                    v.ReferencePrice = ROUND(v.ReferencePriceUsd * COALESCE(r.USDToCurrency, 1), 4),
                    v.CompareAtPrice = CASE WHEN v.CompareAtPriceUsd IS NULL THEN NULL
                        ELSE ROUND(v.CompareAtPriceUsd * COALESCE(r.USDToCurrency, 1), 4) END
                FROM ProductVariants v
                OUTER APPLY (
                    SELECT TOP 1 cr.USDToCurrency
                    FROM CurrencyRates cr
                    WHERE cr.WebsiteID = v.WebsiteID
                    ORDER BY CASE WHEN cr.IsDefault = 1 THEN 0 ELSE 1 END, cr.CurrencyRateID
                ) r;

                UPDATE vp SET
                    vp.ReferencePrice = ROUND(vp.ReferencePriceUsd * COALESCE(r.USDToCurrency, 1), 4),
                    vp.OverridePriceLocal = CASE WHEN vp.OverridePrice IS NULL THEN NULL
                        ELSE ROUND(vp.OverridePrice * COALESCE(r.USDToCurrency, 1), 4) END
                FROM VendorProducts vp
                OUTER APPLY (
                    SELECT TOP 1 cr.USDToCurrency
                    FROM CurrencyRates cr
                    WHERE cr.WebsiteID = vp.WebsiteID
                    ORDER BY CASE WHEN cr.IsDefault = 1 THEN 0 ELSE 1 END, cr.CurrencyRateID
                ) r;

                UPDATE h SET
                    h.ReferencePrice = ROUND(h.ReferencePriceUsd * COALESCE(r.USDToCurrency, 1), 4),
                    h.CompareAtPrice = CASE WHEN h.CompareAtPriceUsd IS NULL THEN NULL
                        ELSE ROUND(h.CompareAtPriceUsd * COALESCE(r.USDToCurrency, 1), 4) END
                FROM ProductVariantPriceHistories h
                INNER JOIN ProductVariants v ON v.ProductVariantID = h.ProductVariantID
                OUTER APPLY (
                    SELECT TOP 1 cr.USDToCurrency
                    FROM CurrencyRates cr
                    WHERE cr.WebsiteID = v.WebsiteID
                    ORDER BY CASE WHEN cr.IsDefault = 1 THEN 0 ELSE 1 END, cr.CurrencyRateID
                ) r;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorePricesInUsd",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "OverridePriceLocal",
                table: "VendorProducts");

            migrationBuilder.DropColumn(
                name: "ReferencePrice",
                table: "VendorProducts");

            migrationBuilder.DropColumn(
                name: "CompareAtPrice",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ReferencePrice",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "CompareAtPrice",
                table: "ProductVariantPriceHistories");

            migrationBuilder.DropColumn(
                name: "ReferencePrice",
                table: "ProductVariantPriceHistories");
        }
    }
}
