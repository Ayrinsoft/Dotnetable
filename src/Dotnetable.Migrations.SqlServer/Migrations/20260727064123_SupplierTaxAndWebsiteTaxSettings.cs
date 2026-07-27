using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class SupplierTaxAndWebsiteTaxSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PricesIncludeTax",
                table: "Websites",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SellerEconomicCode",
                table: "Websites",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerLegalName",
                table: "Websites",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerRegistrationNumber",
                table: "Websites",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerTaxId",
                table: "Websites",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerVatNumber",
                table: "Websites",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaxCountryID",
                table: "Websites",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TaxEnabled",
                table: "Websites",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "TaxOnShipping",
                table: "Websites",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyToShipping",
                table: "TaxRates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TaxCode",
                table: "TaxRates",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "TaxKind",
                table: "TaxRates",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine",
                table: "Suppliers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "Suppliers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankIban",
                table: "Suppliers",
                type: "nvarchar(34)",
                maxLength: 34,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Suppliers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CityName",
                table: "Suppliers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountryID",
                table: "Suppliers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Suppliers",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DefaultCurrencyCode",
                table: "Suppliers",
                type: "char(3)",
                unicode: false,
                fixedLength: true,
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EconomicCode",
                table: "Suppliers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Suppliers",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVatRegistered",
                table: "Suppliers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                table: "Suppliers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LinkedVendorID",
                table: "Suppliers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LinkedWebsiteID",
                table: "Suppliers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Suppliers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Suppliers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "Suppliers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "SupplierType",
                table: "Suppliers",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "TaxIdentificationNumber",
                table: "Suppliers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatNumber",
                table: "Suppliers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                table: "Settlements",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "Settlements",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRateSnapshot",
                table: "Settlements",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PricesIncludeTax",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TaxBreakdownJson",
                table: "Orders",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Websites_TaxCountryID",
                table: "Websites",
                column: "TaxCountryID");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CountryID",
                table: "Suppliers",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_DefaultCurrencyCode",
                table: "Suppliers",
                column: "DefaultCurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_LinkedVendorID",
                table: "Suppliers",
                column: "LinkedVendorID");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_LinkedWebsiteID",
                table: "Suppliers",
                column: "LinkedWebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Countries",
                table: "Suppliers",
                column: "CountryID",
                principalTable: "Countries",
                principalColumn: "CountryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Currencies",
                table: "Suppliers",
                column: "DefaultCurrencyCode",
                principalTable: "Currencies",
                principalColumn: "CurrencyCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_LinkedWebsites",
                table: "Suppliers",
                column: "LinkedWebsiteID",
                principalTable: "Websites",
                principalColumn: "WebsiteID");

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Vendors",
                table: "Suppliers",
                column: "LinkedVendorID",
                principalTable: "Vendors",
                principalColumn: "VendorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Websites_TaxCountries",
                table: "Websites",
                column: "TaxCountryID",
                principalTable: "Countries",
                principalColumn: "CountryID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Countries",
                table: "Suppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Currencies",
                table: "Suppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_LinkedWebsites",
                table: "Suppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Vendors",
                table: "Suppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_Websites_TaxCountries",
                table: "Websites");

            migrationBuilder.DropIndex(
                name: "IX_Websites_TaxCountryID",
                table: "Websites");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_CountryID",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_DefaultCurrencyCode",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_LinkedVendorID",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_LinkedWebsiteID",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "PricesIncludeTax",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "SellerEconomicCode",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "SellerLegalName",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "SellerRegistrationNumber",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "SellerTaxId",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "SellerVatNumber",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "TaxCountryID",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "TaxEnabled",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "TaxOnShipping",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "ApplyToShipping",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "TaxCode",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "TaxKind",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "AddressLine",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "BankIban",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CityName",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CountryID",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "DefaultCurrencyCode",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "EconomicCode",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "IsVatRegistered",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "LegalName",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "LinkedVendorID",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "LinkedWebsiteID",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "SupplierType",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "TaxIdentificationNumber",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "VatNumber",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "NetAmount",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "TaxRateSnapshot",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "PricesIncludeTax",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TaxBreakdownJson",
                table: "Orders");
        }
    }
}
