using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    [Migration("20260812200010_CountVarianceTaxPeriod")]
    public class CountVarianceTaxPeriod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE ""StockDocumentLines"" ADD COLUMN IF NOT EXISTS ""BookQuantity"" INT NOT NULL DEFAULT 0;
ALTER TABLE ""StockDocumentLines"" ADD COLUMN IF NOT EXISTS ""CountedQuantity"" INT NULL;

CREATE TABLE IF NOT EXISTS ""TaxPeriods"" (
  ""TaxPeriodID"" SERIAL PRIMARY KEY,
  ""WebsiteID"" INT NOT NULL REFERENCES ""Websites""(""WebsiteID""),
  ""PeriodCode"" VARCHAR(30) NOT NULL,
  ""FromDate"" DATE NOT NULL,
  ""ToDate"" DATE NOT NULL,
  ""Status"" SMALLINT NOT NULL,
  ""OutputTaxSnapshot"" NUMERIC(18,4) NOT NULL DEFAULT 0,
  ""SettlementTaxSnapshot"" NUMERIC(18,4) NOT NULL DEFAULT 0,
  ""NetTaxSnapshot"" NUMERIC(18,4) NOT NULL DEFAULT 0,
  ""OutputOrderCount"" INT NOT NULL DEFAULT 0,
  ""SettlementCount"" INT NOT NULL DEFAULT 0,
  ""Note"" VARCHAR(1000) NULL,
  ""CreatedByMemberID"" INT NULL REFERENCES ""Members""(""MemberID""),
  ""ClosedByMemberID"" INT NULL REFERENCES ""Members""(""MemberID""),
  ""ClosedAt"" TIMESTAMP WITHOUT TIME ZONE NULL,
  ""SnapshotAt"" TIMESTAMP WITHOUT TIME ZONE NULL,
  ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_TaxPeriods_Website_Code"" ON ""TaxPeriods"" (""WebsiteID"", ""PeriodCode"");
CREATE INDEX IF NOT EXISTS ""IX_TaxPeriods_Website_Status"" ON ""TaxPeriods"" (""WebsiteID"", ""Status"", ""FromDate"");
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TABLE IF EXISTS ""TaxPeriods"";
ALTER TABLE ""StockDocumentLines"" DROP COLUMN IF EXISTS ""CountedQuantity"";
ALTER TABLE ""StockDocumentLines"" DROP COLUMN IF EXISTS ""BookQuantity"";
");
        }
    }
}
