using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    [Migration("20260812200020_CountVarianceTaxPeriod")]
    public class CountVarianceTaxPeriod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @db := DATABASE();
SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='StockDocumentLines' AND COLUMN_NAME='BookQuantity');
SET @s := IF(@e=0, 'ALTER TABLE StockDocumentLines ADD COLUMN BookQuantity INT NOT NULL DEFAULT 0', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;
SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='StockDocumentLines' AND COLUMN_NAME='CountedQuantity');
SET @s := IF(@e=0, 'ALTER TABLE StockDocumentLines ADD COLUMN CountedQuantity INT NULL', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;

CREATE TABLE IF NOT EXISTS TaxPeriods (
  TaxPeriodID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  WebsiteID INT NOT NULL,
  PeriodCode VARCHAR(30) NOT NULL,
  FromDate DATE NOT NULL,
  ToDate DATE NOT NULL,
  Status TINYINT NOT NULL,
  OutputTaxSnapshot DECIMAL(18,4) NOT NULL DEFAULT 0,
  SettlementTaxSnapshot DECIMAL(18,4) NOT NULL DEFAULT 0,
  NetTaxSnapshot DECIMAL(18,4) NOT NULL DEFAULT 0,
  OutputOrderCount INT NOT NULL DEFAULT 0,
  SettlementCount INT NOT NULL DEFAULT 0,
  Note VARCHAR(1000) NULL,
  CreatedByMemberID INT NULL,
  ClosedByMemberID INT NULL,
  ClosedAt DATETIME(6) NULL,
  SnapshotAt DATETIME(6) NULL,
  CreatedAt DATETIME(6) NOT NULL,
  UNIQUE KEY IX_TaxPeriods_Website_Code (WebsiteID, PeriodCode),
  KEY IX_TaxPeriods_Website_Status (WebsiteID, Status, FromDate),
  CONSTRAINT FK_TaxPeriods_Websites FOREIGN KEY (WebsiteID) REFERENCES Websites(WebsiteID),
  CONSTRAINT FK_TaxPeriods_CreatedBy FOREIGN KEY (CreatedByMemberID) REFERENCES Members(MemberID),
  CONSTRAINT FK_TaxPeriods_ClosedBy FOREIGN KEY (ClosedByMemberID) REFERENCES Members(MemberID)
);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TABLE IF EXISTS TaxPeriods;
SET @db := DATABASE();
SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='StockDocumentLines' AND COLUMN_NAME='CountedQuantity');
SET @s := IF(@e>0, 'ALTER TABLE StockDocumentLines DROP COLUMN CountedQuantity', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;
SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='StockDocumentLines' AND COLUMN_NAME='BookQuantity');
SET @s := IF(@e>0, 'ALTER TABLE StockDocumentLines DROP COLUMN BookQuantity', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;
");
        }
    }
}
