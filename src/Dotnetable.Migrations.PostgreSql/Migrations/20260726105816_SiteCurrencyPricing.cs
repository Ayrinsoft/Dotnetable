using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
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
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "When true, also persist USD dual columns; default site currency is always operational authority.");

            migrationBuilder.AddColumn<decimal>(
                name: "OverridePriceLocal",
                table: "VendorProducts",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReferencePrice",
                table: "VendorProducts",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CompareAtPrice",
                table: "ProductVariants",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReferencePrice",
                table: "ProductVariants",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CompareAtPrice",
                table: "ProductVariantPriceHistories",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReferencePrice",
                table: "ProductVariantPriceHistories",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                UPDATE "ProductVariants" v
                SET
                    "ReferencePrice" = ROUND(v."ReferencePriceUsd" * COALESCE(r."USDToCurrency", 1), 4),
                    "CompareAtPrice" = CASE WHEN v."CompareAtPriceUsd" IS NULL THEN NULL
                        ELSE ROUND(v."CompareAtPriceUsd" * COALESCE(r."USDToCurrency", 1), 4) END
                FROM (
                    SELECT DISTINCT ON ("WebsiteID") "WebsiteID", "USDToCurrency"
                    FROM "CurrencyRates"
                    ORDER BY "WebsiteID", "IsDefault" DESC, "CurrencyRateID"
                ) r
                WHERE r."WebsiteID" = v."WebsiteID";

                UPDATE "ProductVariants"
                SET "ReferencePrice" = "ReferencePriceUsd"
                WHERE "ReferencePrice" = 0 AND "ReferencePriceUsd" <> 0;

                UPDATE "VendorProducts" vp
                SET
                    "ReferencePrice" = ROUND(vp."ReferencePriceUsd" * COALESCE(r."USDToCurrency", 1), 4),
                    "OverridePriceLocal" = CASE WHEN vp."OverridePrice" IS NULL THEN NULL
                        ELSE ROUND(vp."OverridePrice" * COALESCE(r."USDToCurrency", 1), 4) END
                FROM (
                    SELECT DISTINCT ON ("WebsiteID") "WebsiteID", "USDToCurrency"
                    FROM "CurrencyRates"
                    ORDER BY "WebsiteID", "IsDefault" DESC, "CurrencyRateID"
                ) r
                WHERE r."WebsiteID" = vp."WebsiteID";

                UPDATE "VendorProducts"
                SET "ReferencePrice" = "ReferencePriceUsd"
                WHERE "ReferencePrice" = 0 AND "ReferencePriceUsd" <> 0;

                UPDATE "ProductVariantPriceHistories" h
                SET
                    "ReferencePrice" = ROUND(h."ReferencePriceUsd" * COALESCE(r."USDToCurrency", 1), 4),
                    "CompareAtPrice" = CASE WHEN h."CompareAtPriceUsd" IS NULL THEN NULL
                        ELSE ROUND(h."CompareAtPriceUsd" * COALESCE(r."USDToCurrency", 1), 4) END
                FROM "ProductVariants" v
                LEFT JOIN (
                    SELECT DISTINCT ON ("WebsiteID") "WebsiteID", "USDToCurrency"
                    FROM "CurrencyRates"
                    ORDER BY "WebsiteID", "IsDefault" DESC, "CurrencyRateID"
                ) r ON r."WebsiteID" = v."WebsiteID"
                WHERE v."ProductVariantID" = h."ProductVariantID";
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
