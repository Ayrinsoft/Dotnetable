using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
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
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                table: "Vendors",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "VendorCreditTransactions",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAfter",
                table: "VendorCreditTransactions",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "StockMovements",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitSalePrice",
                table: "StockMovements",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ShippingRates",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AvgCost",
                table: "InventoryItems",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxDiscountAmount",
                table: "Coupons",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinOrderAmount",
                table: "Coupons",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "CouponRedemptions",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "ClientWalletWithdrawals",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "ClientWalletTransactions",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAfter",
                table: "ClientWalletTransactions",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "ClientWallets",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                UPDATE "Vendors" SET "AvailableCredit" = "AvailableCreditUsd", "CreditLimit" = "CreditLimitUsd";
                UPDATE "VendorCreditTransactions" SET "Amount" = "AmountUsd", "BalanceAfter" = "BalanceAfterUsd";
                UPDATE "StockMovements" SET "UnitCost" = "UnitCostUsd", "UnitSalePrice" = "UnitSalePriceUsd";
                UPDATE "ShippingRates" SET "Price" = "PriceUsd";
                UPDATE "InventoryItems" SET "AvgCost" = "AvgCostUsd";
                UPDATE "Coupons" SET "MinOrderAmount" = "MinOrderAmountUsd", "MaxDiscountAmount" = "MaxDiscountAmountUsd";
                UPDATE "CouponRedemptions" SET "DiscountAmount" = "DiscountAmountUsd";
                UPDATE "ClientWalletWithdrawals" SET "Amount" = "AmountUsd";
                UPDATE "ClientWalletTransactions" SET "Amount" = "AmountUsd", "BalanceAfter" = "BalanceAfterUsd";
                UPDATE "ClientWallets" SET "Balance" = "BalanceUsd";
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
