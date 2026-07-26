using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class SiteCurrencyMoneyEverywhere : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AvailableCredit",
                table: "Vendors",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                table: "Vendors",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "VendorCreditTransactions",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAfter",
                table: "VendorCreditTransactions",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "StockMovements",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitSalePrice",
                table: "StockMovements",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ShippingRates",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AvgCost",
                table: "InventoryItems",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxDiscountAmount",
                table: "Coupons",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinOrderAmount",
                table: "Coupons",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "CouponRedemptions",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "ClientWalletWithdrawals",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "ClientWalletTransactions",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAfter",
                table: "ClientWalletTransactions",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "ClientWallets",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            // Backfill site-currency from USD (× default rate when available; else 1:1).
            migrationBuilder.Sql("""
                UPDATE v SET
                    v.AvailableCredit = ROUND(v.AvailableCreditUsd * COALESCE(r.USDToCurrency, 1), 4),
                    v.CreditLimit = CASE WHEN v.CreditLimitUsd IS NULL THEN NULL
                        ELSE ROUND(v.CreditLimitUsd * COALESCE(r.USDToCurrency, 1), 4) END
                FROM Vendors v
                OUTER APPLY (
                    SELECT TOP 1 cr.USDToCurrency FROM CurrencyRates cr
                    WHERE cr.WebsiteID = v.WebsiteID
                    ORDER BY CASE WHEN cr.IsDefault = 1 THEN 0 ELSE 1 END, cr.CurrencyRateID
                ) r;

                UPDATE t SET
                    t.Amount = ROUND(t.AmountUsd * COALESCE(r.USDToCurrency, 1), 4),
                    t.BalanceAfter = ROUND(t.BalanceAfterUsd * COALESCE(r.USDToCurrency, 1), 4)
                FROM VendorCreditTransactions t
                OUTER APPLY (
                    SELECT TOP 1 cr.USDToCurrency FROM CurrencyRates cr
                    WHERE cr.WebsiteID = t.WebsiteID
                    ORDER BY CASE WHEN cr.IsDefault = 1 THEN 0 ELSE 1 END, cr.CurrencyRateID
                ) r;

                UPDATE sm SET
                    sm.UnitCost = ROUND(sm.UnitCostUsd * CASE WHEN sm.ExchangeRateToUsd > 0 THEN sm.ExchangeRateToUsd ELSE COALESCE(r.USDToCurrency, 1) END, 4),
                    sm.UnitSalePrice = CASE WHEN sm.UnitSalePriceUsd IS NULL THEN NULL
                        ELSE ROUND(sm.UnitSalePriceUsd * CASE WHEN sm.ExchangeRateToUsd > 0 THEN sm.ExchangeRateToUsd ELSE COALESCE(r.USDToCurrency, 1) END, 4) END
                FROM StockMovements sm
                OUTER APPLY (
                    SELECT TOP 1 cr.USDToCurrency FROM CurrencyRates cr
                    WHERE cr.WebsiteID = sm.WebsiteID
                    ORDER BY CASE WHEN cr.IsDefault = 1 THEN 0 ELSE 1 END, cr.CurrencyRateID
                ) r;

                UPDATE sr SET sr.Price = ROUND(sr.PriceUsd * COALESCE(r.USDToCurrency, 1), 4)
                FROM ShippingRates sr
                INNER JOIN ShippingMethods m ON m.ShippingMethodID = sr.ShippingMethodID
                OUTER APPLY (
                    SELECT TOP 1 cr.USDToCurrency FROM CurrencyRates cr
                    WHERE cr.WebsiteID = m.WebsiteID
                    ORDER BY CASE WHEN cr.IsDefault = 1 THEN 0 ELSE 1 END, cr.CurrencyRateID
                ) r;

                UPDATE i SET i.AvgCost = ROUND(i.AvgCostUsd * COALESCE(r.USDToCurrency, 1), 4)
                FROM InventoryItems i
                OUTER APPLY (
                    SELECT TOP 1 cr.USDToCurrency FROM CurrencyRates cr
                    WHERE cr.WebsiteID = i.WebsiteID
                    ORDER BY CASE WHEN cr.IsDefault = 1 THEN 0 ELSE 1 END, cr.CurrencyRateID
                ) r;

                UPDATE c SET
                    c.MinOrderAmount = ROUND(c.MinOrderAmountUsd * COALESCE(r.USDToCurrency, 1), 4),
                    c.MaxDiscountAmount = CASE WHEN c.MaxDiscountAmountUsd IS NULL THEN NULL
                        ELSE ROUND(c.MaxDiscountAmountUsd * COALESCE(r.USDToCurrency, 1), 4) END
                FROM Coupons c
                OUTER APPLY (
                    SELECT TOP 1 cr.USDToCurrency FROM CurrencyRates cr
                    WHERE cr.WebsiteID = c.WebsiteID
                    ORDER BY CASE WHEN cr.IsDefault = 1 THEN 0 ELSE 1 END, cr.CurrencyRateID
                ) r;

                UPDATE cr SET cr.DiscountAmount = ROUND(cr.DiscountAmountUsd * COALESCE(r.USDToCurrency, 1), 4)
                FROM CouponRedemptions cr
                INNER JOIN Coupons c ON c.CouponID = cr.CouponID
                OUTER APPLY (
                    SELECT TOP 1 x.USDToCurrency FROM CurrencyRates x
                    WHERE x.WebsiteID = c.WebsiteID
                    ORDER BY CASE WHEN x.IsDefault = 1 THEN 0 ELSE 1 END, x.CurrencyRateID
                ) r;

                UPDATE w SET w.Amount = ROUND(w.AmountUsd * COALESCE(r.USDToCurrency, 1), 4)
                FROM ClientWalletWithdrawals w
                OUTER APPLY (
                    SELECT TOP 1 cr.USDToCurrency FROM CurrencyRates cr
                    WHERE cr.WebsiteID = w.WebsiteID
                    ORDER BY CASE WHEN cr.IsDefault = 1 THEN 0 ELSE 1 END, cr.CurrencyRateID
                ) r;

                UPDATE t SET
                    t.Amount = ROUND(t.AmountUsd * COALESCE(r.USDToCurrency, 1), 4),
                    t.BalanceAfter = ROUND(t.BalanceAfterUsd * COALESCE(r.USDToCurrency, 1), 4)
                FROM ClientWalletTransactions t
                OUTER APPLY (
                    SELECT TOP 1 cr.USDToCurrency FROM CurrencyRates cr
                    WHERE cr.WebsiteID = t.WebsiteID
                    ORDER BY CASE WHEN cr.IsDefault = 1 THEN 0 ELSE 1 END, cr.CurrencyRateID
                ) r;

                UPDATE w SET w.Balance = ROUND(w.BalanceUsd * COALESCE(r.USDToCurrency, 1), 4)
                FROM ClientWallets w
                OUTER APPLY (
                    SELECT TOP 1 cr.USDToCurrency FROM CurrencyRates cr
                    WHERE cr.WebsiteID = w.WebsiteID
                    ORDER BY CASE WHEN cr.IsDefault = 1 THEN 0 ELSE 1 END, cr.CurrencyRateID
                ) r;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableCredit",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "CreditLimit",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "VendorCreditTransactions");

            migrationBuilder.DropColumn(
                name: "BalanceAfter",
                table: "VendorCreditTransactions");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "UnitSalePrice",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ShippingRates");

            migrationBuilder.DropColumn(
                name: "AvgCost",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "MaxDiscountAmount",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "MinOrderAmount",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "CouponRedemptions");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "ClientWalletWithdrawals");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "ClientWalletTransactions");

            migrationBuilder.DropColumn(
                name: "BalanceAfter",
                table: "ClientWalletTransactions");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "ClientWallets");
        }
    }
}
