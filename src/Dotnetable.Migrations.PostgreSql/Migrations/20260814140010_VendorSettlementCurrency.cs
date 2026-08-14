using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    [Migration("20260814140010_VendorSettlementCurrency")]
    public class VendorSettlementCurrency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE ""Vendors"" ADD COLUMN IF NOT EXISTS ""SettlementCurrencyCode"" CHAR(3) NULL;
CREATE INDEX IF NOT EXISTS ""IX_Vendors_SettlementCurrencyCode"" ON ""Vendors"" (""SettlementCurrencyCode"");
DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Vendors_Currencies_Settlement') THEN
    ALTER TABLE ""Vendors"" ADD CONSTRAINT ""FK_Vendors_Currencies_Settlement""
      FOREIGN KEY (""SettlementCurrencyCode"") REFERENCES ""Currencies"" (""CurrencyCode"");
  END IF;
END $$;
ALTER TABLE ""Settlements"" ADD COLUMN IF NOT EXISTS ""SourceCurrencyCode"" CHAR(3) NULL;
ALTER TABLE ""Settlements"" ADD COLUMN IF NOT EXISTS ""SourceNetAmount"" DECIMAL(18,4) NOT NULL DEFAULT 0;
ALTER TABLE ""Settlements"" ADD COLUMN IF NOT EXISTS ""SourceTaxAmount"" DECIMAL(18,4) NOT NULL DEFAULT 0;
ALTER TABLE ""Settlements"" ADD COLUMN IF NOT EXISTS ""SourceTotalAmount"" DECIMAL(18,4) NOT NULL DEFAULT 0;
ALTER TABLE ""Settlements"" ADD COLUMN IF NOT EXISTS ""BridgeUsdAmount"" DECIMAL(18,4) NOT NULL DEFAULT 0;
ALTER TABLE ""Settlements"" ADD COLUMN IF NOT EXISTS ""ExchangeRateToUsd"" DECIMAL(18,6) NULL;
ALTER TABLE ""Settlements"" ADD COLUMN IF NOT EXISTS ""ExchangeRateUsdToSettle"" DECIMAL(18,6) NULL;
CREATE INDEX IF NOT EXISTS ""IX_Settlements_SourceCurrencyCode"" ON ""Settlements"" (""SourceCurrencyCode"");
DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Settlements_Currencies_Source') THEN
    ALTER TABLE ""Settlements"" ADD CONSTRAINT ""FK_Settlements_Currencies_Source""
      FOREIGN KEY (""SourceCurrencyCode"") REFERENCES ""Currencies"" (""CurrencyCode"");
  END IF;
END $$;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE ""Settlements"" DROP CONSTRAINT IF EXISTS ""FK_Settlements_Currencies_Source"";
DROP INDEX IF EXISTS ""IX_Settlements_SourceCurrencyCode"";
ALTER TABLE ""Settlements"" DROP COLUMN IF EXISTS ""SourceCurrencyCode"";
ALTER TABLE ""Settlements"" DROP COLUMN IF EXISTS ""SourceNetAmount"";
ALTER TABLE ""Settlements"" DROP COLUMN IF EXISTS ""SourceTaxAmount"";
ALTER TABLE ""Settlements"" DROP COLUMN IF EXISTS ""SourceTotalAmount"";
ALTER TABLE ""Settlements"" DROP COLUMN IF EXISTS ""BridgeUsdAmount"";
ALTER TABLE ""Settlements"" DROP COLUMN IF EXISTS ""ExchangeRateToUsd"";
ALTER TABLE ""Settlements"" DROP COLUMN IF EXISTS ""ExchangeRateUsdToSettle"";
ALTER TABLE ""Vendors"" DROP CONSTRAINT IF EXISTS ""FK_Vendors_Currencies_Settlement"";
DROP INDEX IF EXISTS ""IX_Vendors_SettlementCurrencyCode"";
ALTER TABLE ""Vendors"" DROP COLUMN IF EXISTS ""SettlementCurrencyCode"";
");
        }
    }
}
