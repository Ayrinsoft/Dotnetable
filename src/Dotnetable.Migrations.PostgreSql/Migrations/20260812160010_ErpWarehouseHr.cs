using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace Dotnetable.Migrations.PostgreSql.Migrations {
public class ErpWarehouseHr : Migration {
protected override void Up(MigrationBuilder migrationBuilder) {
migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS ""Warehouses"" (
  ""WarehouseID"" SERIAL PRIMARY KEY,
  ""WebsiteID"" INT NOT NULL REFERENCES ""Websites""(""WebsiteID""),
  ""Code"" VARCHAR(20) NOT NULL,
  ""Name"" VARCHAR(200) NOT NULL,
  ""Address"" VARCHAR(500) NULL,
  ""IsDefault"" BOOLEAN NOT NULL DEFAULT FALSE,
  ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
  ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
  UNIQUE (""WebsiteID"", ""Code"")
);
CREATE TABLE IF NOT EXISTS ""WarehouseStocks"" (
  ""WarehouseStockID"" SERIAL PRIMARY KEY,
  ""WarehouseID"" INT NOT NULL REFERENCES ""Warehouses""(""WarehouseID""),
  ""ProductVariantID"" INT NOT NULL REFERENCES ""ProductVariants""(""ProductVariantID""),
  ""QuantityOnHand"" INT NOT NULL,
  ""QuantityReserved"" INT NOT NULL DEFAULT 0,
  ""RowVersion"" BYTEA NOT NULL DEFAULT E'\\x00',
  UNIQUE (""WarehouseID"", ""ProductVariantID"")
);
CREATE TABLE IF NOT EXISTS ""StockDocuments"" (
  ""StockDocumentID"" SERIAL PRIMARY KEY,
  ""WebsiteID"" INT NOT NULL REFERENCES ""Websites""(""WebsiteID""),
  ""DocumentNumber"" VARCHAR(40) NOT NULL,
  ""DocumentType"" SMALLINT NOT NULL,
  ""Status"" SMALLINT NOT NULL,
  ""FromWarehouseID"" INT NULL REFERENCES ""Warehouses""(""WarehouseID""),
  ""ToWarehouseID"" INT NULL REFERENCES ""Warehouses""(""WarehouseID""),
  ""SupplierID"" INT NULL,
  ""OrderID"" INT NULL,
  ""Note"" VARCHAR(1000) NULL,
  ""RequestedByMemberID"" INT NULL,
  ""ApprovedByMemberID"" INT NULL,
  ""PostedByMemberID"" INT NULL,
  ""SubmittedAt"" TIMESTAMP WITHOUT TIME ZONE NULL,
  ""ApprovedAt"" TIMESTAMP WITHOUT TIME ZONE NULL,
  ""PostedAt"" TIMESTAMP WITHOUT TIME ZONE NULL,
  ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL
);
CREATE TABLE IF NOT EXISTS ""StockDocumentLines"" (
  ""StockDocumentLineID"" SERIAL PRIMARY KEY,
  ""StockDocumentID"" INT NOT NULL REFERENCES ""StockDocuments""(""StockDocumentID""),
  ""ProductVariantID"" INT NOT NULL,
  ""Quantity"" INT NOT NULL,
  ""UnitCost"" NUMERIC(18,4) NOT NULL,
  ""UnitCostUsd"" NUMERIC(18,4) NOT NULL,
  ""Note"" VARCHAR(300) NULL
);
CREATE TABLE IF NOT EXISTS ""StockDocumentHistories"" (
  ""StockDocumentHistoryID"" SERIAL PRIMARY KEY,
  ""StockDocumentID"" INT NOT NULL REFERENCES ""StockDocuments""(""StockDocumentID""),
  ""FromStatus"" SMALLINT NOT NULL,
  ""ToStatus"" SMALLINT NOT NULL,
  ""Note"" VARCHAR(500) NULL,
  ""CreatedByMemberID"" INT NULL,
  ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL
);
CREATE TABLE IF NOT EXISTS ""OrgUnits"" (
  ""OrgUnitID"" SERIAL PRIMARY KEY,
  ""WebsiteID"" INT NOT NULL REFERENCES ""Websites""(""WebsiteID""),
  ""ParentOrgUnitID"" INT NULL,
  ""Code"" VARCHAR(20) NOT NULL,
  ""Name"" VARCHAR(200) NOT NULL,
  ""SortOrder"" INT NOT NULL DEFAULT 0,
  ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
  UNIQUE (""WebsiteID"", ""Code"")
);
CREATE TABLE IF NOT EXISTS ""Employees"" (
  ""EmployeeID"" SERIAL PRIMARY KEY,
  ""WebsiteID"" INT NOT NULL REFERENCES ""Websites""(""WebsiteID""),
  ""EmployeeCode"" VARCHAR(30) NOT NULL,
  ""GivenName"" VARCHAR(100) NOT NULL,
  ""Surname"" VARCHAR(100) NOT NULL,
  ""NationalId"" VARCHAR(50) NULL,
  ""Email"" VARCHAR(200) NULL,
  ""Phone"" VARCHAR(50) NULL,
  ""OrgUnitID"" INT NULL,
  ""JobTitle"" VARCHAR(150) NULL,
  ""MemberID"" INT NULL,
  ""Status"" SMALLINT NOT NULL DEFAULT 1,
  ""HireDate"" DATE NOT NULL,
  ""TerminationDate"" DATE NULL,
  ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
  UNIQUE (""WebsiteID"", ""EmployeeCode"")
);
CREATE TABLE IF NOT EXISTS ""EmployeeContracts"" (
  ""EmployeeContractID"" SERIAL PRIMARY KEY,
  ""EmployeeID"" INT NOT NULL REFERENCES ""Employees""(""EmployeeID""),
  ""EffectiveFrom"" DATE NOT NULL,
  ""EffectiveTo"" DATE NULL,
  ""BaseSalary"" NUMERIC(18,4) NOT NULL,
  ""CurrencyCode"" CHAR(3) NOT NULL,
  ""PayFrequency"" SMALLINT NOT NULL DEFAULT 1,
  ""EmployeeInsuranceRate"" NUMERIC(9,6) NOT NULL,
  ""EmployerInsuranceRate"" NUMERIC(9,6) NOT NULL,
  ""IncomeTaxRate"" NUMERIC(9,6) NOT NULL,
  ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
  ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL
);
CREATE TABLE IF NOT EXISTS ""PayrollRuns"" (
  ""PayrollRunID"" SERIAL PRIMARY KEY,
  ""WebsiteID"" INT NOT NULL REFERENCES ""Websites""(""WebsiteID""),
  ""RunNumber"" VARCHAR(40) NOT NULL,
  ""PeriodFrom"" DATE NOT NULL,
  ""PeriodTo"" DATE NOT NULL,
  ""Status"" SMALLINT NOT NULL,
  ""TotalGross"" NUMERIC(18,4) NOT NULL,
  ""TotalEmployeeInsurance"" NUMERIC(18,4) NOT NULL,
  ""TotalEmployerInsurance"" NUMERIC(18,4) NOT NULL,
  ""TotalIncomeTax"" NUMERIC(18,4) NOT NULL,
  ""TotalNet"" NUMERIC(18,4) NOT NULL,
  ""CurrencyCode"" CHAR(3) NOT NULL,
  ""Note"" VARCHAR(1000) NULL,
  ""CreatedByMemberID"" INT NULL,
  ""ApprovedByMemberID"" INT NULL,
  ""ApprovedAt"" TIMESTAMP WITHOUT TIME ZONE NULL,
  ""PaidAt"" TIMESTAMP WITHOUT TIME ZONE NULL,
  ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL
);
CREATE TABLE IF NOT EXISTS ""PayrollLines"" (
  ""PayrollLineID"" SERIAL PRIMARY KEY,
  ""PayrollRunID"" INT NOT NULL REFERENCES ""PayrollRuns""(""PayrollRunID""),
  ""EmployeeID"" INT NOT NULL REFERENCES ""Employees""(""EmployeeID""),
  ""Gross"" NUMERIC(18,4) NOT NULL,
  ""EmployeeInsurance"" NUMERIC(18,4) NOT NULL,
  ""EmployerInsurance"" NUMERIC(18,4) NOT NULL,
  ""IncomeTax"" NUMERIC(18,4) NOT NULL,
  ""Net"" NUMERIC(18,4) NOT NULL,
  ""EmployerCost"" NUMERIC(18,4) NOT NULL,
  ""Note"" VARCHAR(300) NULL
);
");
}
protected override void Down(MigrationBuilder migrationBuilder) {
migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""PayrollLines"",""PayrollRuns"",""EmployeeContracts"",""Employees"",""OrgUnits"",""StockDocumentHistories"",""StockDocumentLines"",""StockDocuments"",""WarehouseStocks"",""Warehouses"" CASCADE;");
}
}}
