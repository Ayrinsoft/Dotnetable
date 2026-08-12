using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    [Migration("20260812180020_ReturnStockAndQc")]
    public class ReturnStockAndQc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @db := DATABASE();
SET @exists := (
  SELECT COUNT(*) FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'StockDocuments' AND COLUMN_NAME = 'PaymentRefundID');
SET @sql := IF(@exists = 0,
  'ALTER TABLE StockDocuments ADD COLUMN PaymentRefundID INT NULL, ADD INDEX IX_StockDocuments_PaymentRefundID (PaymentRefundID), ADD CONSTRAINT FK_StockDocuments_PaymentRefunds FOREIGN KEY (PaymentRefundID) REFERENCES PaymentRefunds(PaymentRefundID)',
  'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exists2 := (
  SELECT COUNT(*) FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'StockDocumentLines' AND COLUMN_NAME = 'ReturnCondition');
SET @sql2 := IF(@exists2 = 0,
  'ALTER TABLE StockDocumentLines ADD COLUMN ReturnCondition TINYINT NOT NULL DEFAULT 0',
  'SELECT 1');
PREPARE stmt2 FROM @sql2; EXECUTE stmt2; DEALLOCATE PREPARE stmt2;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @db := DATABASE();
SET @fk := (
  SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
  WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'StockDocuments' AND CONSTRAINT_NAME = 'FK_StockDocuments_PaymentRefunds');
SET @sql := IF(@fk > 0, 'ALTER TABLE StockDocuments DROP FOREIGN KEY FK_StockDocuments_PaymentRefunds', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @col := (
  SELECT COUNT(*) FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'StockDocuments' AND COLUMN_NAME = 'PaymentRefundID');
SET @sql2 := IF(@col > 0, 'ALTER TABLE StockDocuments DROP COLUMN PaymentRefundID', 'SELECT 1');
PREPARE stmt2 FROM @sql2; EXECUTE stmt2; DEALLOCATE PREPARE stmt2;

SET @col2 := (
  SELECT COUNT(*) FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'StockDocumentLines' AND COLUMN_NAME = 'ReturnCondition');
SET @sql3 := IF(@col2 > 0, 'ALTER TABLE StockDocumentLines DROP COLUMN ReturnCondition', 'SELECT 1');
PREPARE stmt3 FROM @sql3; EXECUTE stmt3; DEALLOCATE PREPARE stmt3;
");
        }
    }
}
