using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    [Migration("20260812190010_ItemConditionAndPayrollBrackets")]
    public class ItemConditionAndPayrollBrackets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE ""StockDocumentLines"" ADD COLUMN IF NOT EXISTS ""HealthGrade"" SMALLINT NOT NULL DEFAULT 0;
ALTER TABLE ""VendorProducts"" ADD COLUMN IF NOT EXISTS ""ItemCondition"" SMALLINT NOT NULL DEFAULT 1;
ALTER TABLE ""VendorProducts"" ADD COLUMN IF NOT EXISTS ""HealthGrade"" SMALLINT NOT NULL DEFAULT 0;
ALTER TABLE ""EmployeeContracts"" ADD COLUMN IF NOT EXISTS ""UseFlatRates"" BOOLEAN NOT NULL DEFAULT TRUE;

CREATE TABLE IF NOT EXISTS ""PayrollRateBrackets"" (
  ""PayrollRateBracketID"" SERIAL PRIMARY KEY,
  ""WebsiteID"" INT NOT NULL REFERENCES ""Websites""(""WebsiteID""),
  ""Kind"" SMALLINT NOT NULL,
  ""FromAmount"" NUMERIC(18,4) NOT NULL,
  ""ToAmount"" NUMERIC(18,4) NULL,
  ""Rate"" NUMERIC(9,6) NOT NULL,
  ""SortOrder"" INT NOT NULL,
  ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
  ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL
);
CREATE INDEX IF NOT EXISTS ""IX_PayrollRateBrackets_Website_Kind""
  ON ""PayrollRateBrackets"" (""WebsiteID"", ""Kind"", ""SortOrder"");
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TABLE IF EXISTS ""PayrollRateBrackets"";
ALTER TABLE ""EmployeeContracts"" DROP COLUMN IF EXISTS ""UseFlatRates"";
ALTER TABLE ""VendorProducts"" DROP COLUMN IF EXISTS ""HealthGrade"";
ALTER TABLE ""VendorProducts"" DROP COLUMN IF EXISTS ""ItemCondition"";
ALTER TABLE ""StockDocumentLines"" DROP COLUMN IF EXISTS ""HealthGrade"";
");
        }
    }
}
