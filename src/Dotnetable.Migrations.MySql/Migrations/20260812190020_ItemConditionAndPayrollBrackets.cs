using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    [Migration("20260812190020_ItemConditionAndPayrollBrackets")]
    public class ItemConditionAndPayrollBrackets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @db := DATABASE();

SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='StockDocumentLines' AND COLUMN_NAME='HealthGrade');
SET @s := IF(@e=0, 'ALTER TABLE StockDocumentLines ADD COLUMN HealthGrade TINYINT NOT NULL DEFAULT 0', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;

SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='VendorProducts' AND COLUMN_NAME='ItemCondition');
SET @s := IF(@e=0, 'ALTER TABLE VendorProducts ADD COLUMN ItemCondition TINYINT NOT NULL DEFAULT 1', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;

SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='VendorProducts' AND COLUMN_NAME='HealthGrade');
SET @s := IF(@e=0, 'ALTER TABLE VendorProducts ADD COLUMN HealthGrade TINYINT NOT NULL DEFAULT 0', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;

SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='EmployeeContracts' AND COLUMN_NAME='UseFlatRates');
SET @s := IF(@e=0, 'ALTER TABLE EmployeeContracts ADD COLUMN UseFlatRates TINYINT(1) NOT NULL DEFAULT 1', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;

CREATE TABLE IF NOT EXISTS PayrollRateBrackets (
  PayrollRateBracketID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  WebsiteID INT NOT NULL,
  Kind TINYINT NOT NULL,
  FromAmount DECIMAL(18,4) NOT NULL,
  ToAmount DECIMAL(18,4) NULL,
  Rate DECIMAL(9,6) NOT NULL,
  SortOrder INT NOT NULL,
  IsActive TINYINT(1) NOT NULL DEFAULT 1,
  CreatedAt DATETIME(6) NOT NULL,
  KEY IX_PayrollRateBrackets_Website_Kind (WebsiteID, Kind, SortOrder),
  CONSTRAINT FK_PayrollRateBrackets_Websites FOREIGN KEY (WebsiteID) REFERENCES Websites(WebsiteID)
);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TABLE IF EXISTS PayrollRateBrackets;
SET @db := DATABASE();
SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='EmployeeContracts' AND COLUMN_NAME='UseFlatRates');
SET @s := IF(@e>0, 'ALTER TABLE EmployeeContracts DROP COLUMN UseFlatRates', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;
SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='VendorProducts' AND COLUMN_NAME='HealthGrade');
SET @s := IF(@e>0, 'ALTER TABLE VendorProducts DROP COLUMN HealthGrade', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;
SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='VendorProducts' AND COLUMN_NAME='ItemCondition');
SET @s := IF(@e>0, 'ALTER TABLE VendorProducts DROP COLUMN ItemCondition', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;
SET @e := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='StockDocumentLines' AND COLUMN_NAME='HealthGrade');
SET @s := IF(@e>0, 'ALTER TABLE StockDocumentLines DROP COLUMN HealthGrade', 'SELECT 1');
PREPARE st FROM @s; EXECUTE st; DEALLOCATE PREPARE st;
");
        }
    }
}
