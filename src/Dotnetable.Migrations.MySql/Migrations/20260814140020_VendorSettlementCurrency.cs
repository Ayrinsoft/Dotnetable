using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    [Migration("20260814140020_VendorSettlementCurrency")]
    public class VendorSettlementCurrency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @db := DATABASE();
SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='Vendors' AND COLUMN_NAME='SettlementCurrencyCode');
SET @s := IF(@e=0, 'ALTER TABLE Vendors ADD COLUMN SettlementCurrencyCode CHAR(3) NULL, ADD KEY IX_Vendors_SettlementCurrencyCode (SettlementCurrencyCode), ADD CONSTRAINT FK_Vendors_Currencies_Settlement FOREIGN KEY (SettlementCurrencyCode) REFERENCES Currencies (CurrencyCode)', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;

SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='Settlements' AND COLUMN_NAME='SourceCurrencyCode');
SET @s := IF(@e=0, 'ALTER TABLE Settlements ADD COLUMN SourceCurrencyCode CHAR(3) NULL, ADD COLUMN SourceNetAmount DECIMAL(18,4) NOT NULL DEFAULT 0, ADD COLUMN SourceTaxAmount DECIMAL(18,4) NOT NULL DEFAULT 0, ADD COLUMN SourceTotalAmount DECIMAL(18,4) NOT NULL DEFAULT 0, ADD COLUMN BridgeUsdAmount DECIMAL(18,4) NOT NULL DEFAULT 0, ADD COLUMN ExchangeRateToUsd DECIMAL(18,6) NULL, ADD COLUMN ExchangeRateUsdToSettle DECIMAL(18,6) NULL, ADD KEY IX_Settlements_SourceCurrencyCode (SourceCurrencyCode), ADD CONSTRAINT FK_Settlements_Currencies_Source FOREIGN KEY (SourceCurrencyCode) REFERENCES Currencies (CurrencyCode)', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @db := DATABASE();
SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='Settlements' AND COLUMN_NAME='SourceCurrencyCode');
SET @s := IF(@e>0, 'ALTER TABLE Settlements DROP FOREIGN KEY FK_Settlements_Currencies_Source, DROP INDEX IX_Settlements_SourceCurrencyCode, DROP COLUMN SourceCurrencyCode, DROP COLUMN SourceNetAmount, DROP COLUMN SourceTaxAmount, DROP COLUMN SourceTotalAmount, DROP COLUMN BridgeUsdAmount, DROP COLUMN ExchangeRateToUsd, DROP COLUMN ExchangeRateUsdToSettle', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;
SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='Vendors' AND COLUMN_NAME='SettlementCurrencyCode');
SET @s := IF(@e>0, 'ALTER TABLE Vendors DROP FOREIGN KEY FK_Vendors_Currencies_Settlement, DROP INDEX IX_Vendors_SettlementCurrencyCode, DROP COLUMN SettlementCurrencyCode', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;
");
        }
    }
}
