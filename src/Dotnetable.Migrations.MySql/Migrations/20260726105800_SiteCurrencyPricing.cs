using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
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
                type: "tinyint(1)",
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

            migrationBuilder.Sql("""
                UPDATE ProductVariants v
                LEFT JOIN (
                    SELECT WebsiteID, USDToCurrency
                    FROM CurrencyRates cr1
                    WHERE CurrencyRateID = (
                        SELECT CurrencyRateID FROM CurrencyRates cr2
                        WHERE cr2.WebsiteID = cr1.WebsiteID
                        ORDER BY cr2.IsDefault DESC, cr2.CurrencyRateID ASC
                        LIMIT 1
                    )
                ) r ON r.WebsiteID = v.WebsiteID
                SET
                    v.ReferencePrice = ROUND(v.ReferencePriceUsd * COALESCE(r.USDToCurrency, 1), 4),
                    v.CompareAtPrice = CASE WHEN v.CompareAtPriceUsd IS NULL THEN NULL
                        ELSE ROUND(v.CompareAtPriceUsd * COALESCE(r.USDToCurrency, 1), 4) END;

                UPDATE VendorProducts vp
                LEFT JOIN (
                    SELECT WebsiteID, USDToCurrency
                    FROM CurrencyRates cr1
                    WHERE CurrencyRateID = (
                        SELECT CurrencyRateID FROM CurrencyRates cr2
                        WHERE cr2.WebsiteID = cr1.WebsiteID
                        ORDER BY cr2.IsDefault DESC, cr2.CurrencyRateID ASC
                        LIMIT 1
                    )
                ) r ON r.WebsiteID = vp.WebsiteID
                SET
                    vp.ReferencePrice = ROUND(vp.ReferencePriceUsd * COALESCE(r.USDToCurrency, 1), 4),
                    vp.OverridePriceLocal = CASE WHEN vp.OverridePrice IS NULL THEN NULL
                        ELSE ROUND(vp.OverridePrice * COALESCE(r.USDToCurrency, 1), 4) END;

                UPDATE ProductVariantPriceHistories h
                INNER JOIN ProductVariants v ON v.ProductVariantID = h.ProductVariantID
                LEFT JOIN (
                    SELECT WebsiteID, USDToCurrency
                    FROM CurrencyRates cr1
                    WHERE CurrencyRateID = (
                        SELECT CurrencyRateID FROM CurrencyRates cr2
                        WHERE cr2.WebsiteID = cr1.WebsiteID
                        ORDER BY cr2.IsDefault DESC, cr2.CurrencyRateID ASC
                        LIMIT 1
                    )
                ) r ON r.WebsiteID = v.WebsiteID
                SET
                    h.ReferencePrice = ROUND(h.ReferencePriceUsd * COALESCE(r.USDToCurrency, 1), 4),
                    h.CompareAtPrice = CASE WHEN h.CompareAtPriceUsd IS NULL THEN NULL
                        ELSE ROUND(h.CompareAtPriceUsd * COALESCE(r.USDToCurrency, 1), 4) END;
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
