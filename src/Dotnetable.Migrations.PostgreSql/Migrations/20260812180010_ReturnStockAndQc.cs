using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    [Migration("20260812180010_ReturnStockAndQc")]
    public class ReturnStockAndQc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE ""StockDocuments"" ADD COLUMN IF NOT EXISTS ""PaymentRefundID"" INT NULL;
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint WHERE conname = 'FK_StockDocuments_PaymentRefunds') THEN
    ALTER TABLE ""StockDocuments""
      ADD CONSTRAINT ""FK_StockDocuments_PaymentRefunds""
      FOREIGN KEY (""PaymentRefundID"") REFERENCES ""PaymentRefunds""(""PaymentRefundID"");
  END IF;
END $$;
CREATE INDEX IF NOT EXISTS ""IX_StockDocuments_PaymentRefundID"" ON ""StockDocuments"" (""PaymentRefundID"");

ALTER TABLE ""StockDocumentLines"" ADD COLUMN IF NOT EXISTS ""ReturnCondition"" SMALLINT NOT NULL DEFAULT 0;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE ""StockDocuments"" DROP CONSTRAINT IF EXISTS ""FK_StockDocuments_PaymentRefunds"";
DROP INDEX IF EXISTS ""IX_StockDocuments_PaymentRefundID"";
ALTER TABLE ""StockDocuments"" DROP COLUMN IF EXISTS ""PaymentRefundID"";
ALTER TABLE ""StockDocumentLines"" DROP COLUMN IF EXISTS ""ReturnCondition"";
");
        }
    }
}
