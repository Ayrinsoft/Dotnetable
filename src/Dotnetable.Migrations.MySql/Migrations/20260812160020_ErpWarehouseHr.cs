using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace Dotnetable.Migrations.MySql.Migrations {
public class ErpWarehouseHr : Migration {
protected override void Up(MigrationBuilder migrationBuilder) {
migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS Warehouses (
  WarehouseID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  WebsiteID INT NOT NULL,
  Code VARCHAR(20) NOT NULL,
  Name VARCHAR(200) NOT NULL,
  Address VARCHAR(500) NULL,
  IsDefault TINYINT(1) NOT NULL DEFAULT 0,
  IsActive TINYINT(1) NOT NULL DEFAULT 1,
  CreatedAt DATETIME(6) NOT NULL,
  UNIQUE KEY IX_Warehouses_Website_Code (WebsiteID, Code),
  CONSTRAINT FK_Warehouses_Websites FOREIGN KEY (WebsiteID) REFERENCES Websites(WebsiteID)
);
CREATE TABLE IF NOT EXISTS WarehouseStocks (
  WarehouseStockID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  WarehouseID INT NOT NULL,
  ProductVariantID INT NOT NULL,
  QuantityOnHand INT NOT NULL,
  QuantityReserved INT NOT NULL DEFAULT 0,
  RowVersion TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  UNIQUE KEY IX_WS_Wh_Var (WarehouseID, ProductVariantID),
  CONSTRAINT FK_WS_Wh FOREIGN KEY (WarehouseID) REFERENCES Warehouses(WarehouseID),
  CONSTRAINT FK_WS_Var FOREIGN KEY (ProductVariantID) REFERENCES ProductVariants(ProductVariantID)
);
CREATE TABLE IF NOT EXISTS StockDocuments (
  StockDocumentID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  WebsiteID INT NOT NULL,
  DocumentNumber VARCHAR(40) NOT NULL,
  DocumentType TINYINT UNSIGNED NOT NULL,
  Status TINYINT UNSIGNED NOT NULL,
  FromWarehouseID INT NULL,
  ToWarehouseID INT NULL,
  SupplierID INT NULL,
  OrderID INT NULL,
  Note VARCHAR(1000) NULL,
  RequestedByMemberID INT NULL,
  ApprovedByMemberID INT NULL,
  PostedByMemberID INT NULL,
  SubmittedAt DATETIME(6) NULL,
  ApprovedAt DATETIME(6) NULL,
  PostedAt DATETIME(6) NULL,
  CreatedAt DATETIME(6) NOT NULL,
  CONSTRAINT FK_SD_Web FOREIGN KEY (WebsiteID) REFERENCES Websites(WebsiteID)
);
CREATE TABLE IF NOT EXISTS StockDocumentLines (
  StockDocumentLineID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  StockDocumentID INT NOT NULL,
  ProductVariantID INT NOT NULL,
  Quantity INT NOT NULL,
  UnitCost DECIMAL(18,4) NOT NULL,
  UnitCostUsd DECIMAL(18,4) NOT NULL,
  Note VARCHAR(300) NULL,
  CONSTRAINT FK_SDL_Doc FOREIGN KEY (StockDocumentID) REFERENCES StockDocuments(StockDocumentID)
);
CREATE TABLE IF NOT EXISTS StockDocumentHistories (
  StockDocumentHistoryID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  StockDocumentID INT NOT NULL,
  FromStatus TINYINT UNSIGNED NOT NULL,
  ToStatus TINYINT UNSIGNED NOT NULL,
  Note VARCHAR(500) NULL,
  CreatedByMemberID INT NULL,
  CreatedAt DATETIME(6) NOT NULL,
  CONSTRAINT FK_SDH_Doc FOREIGN KEY (StockDocumentID) REFERENCES StockDocuments(StockDocumentID)
);
CREATE TABLE IF NOT EXISTS OrgUnits (
  OrgUnitID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  WebsiteID INT NOT NULL,
  ParentOrgUnitID INT NULL,
  Code VARCHAR(20) NOT NULL,
  Name VARCHAR(200) NOT NULL,
  SortOrder INT NOT NULL DEFAULT 0,
  IsActive TINYINT(1) NOT NULL DEFAULT 1,
  UNIQUE KEY IX_OU_Code (WebsiteID, Code),
  CONSTRAINT FK_OU_Web FOREIGN KEY (WebsiteID) REFERENCES Websites(WebsiteID)
);
CREATE TABLE IF NOT EXISTS Employees (
  EmployeeID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  WebsiteID INT NOT NULL,
  EmployeeCode VARCHAR(30) NOT NULL,
  GivenName VARCHAR(100) NOT NULL,
  Surname VARCHAR(100) NOT NULL,
  NationalId VARCHAR(50) NULL,
  Email VARCHAR(200) NULL,
  Phone VARCHAR(50) NULL,
  OrgUnitID INT NULL,
  JobTitle VARCHAR(150) NULL,
  MemberID INT NULL,
  Status TINYINT UNSIGNED NOT NULL DEFAULT 1,
  HireDate DATE NOT NULL,
  TerminationDate DATE NULL,
  CreatedAt DATETIME(6) NOT NULL,
  UNIQUE KEY IX_Emp_Code (WebsiteID, EmployeeCode),
  CONSTRAINT FK_Emp_Web FOREIGN KEY (WebsiteID) REFERENCES Websites(WebsiteID)
);
CREATE TABLE IF NOT EXISTS EmployeeContracts (
  EmployeeContractID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  EmployeeID INT NOT NULL,
  EffectiveFrom DATE NOT NULL,
  EffectiveTo DATE NULL,
  BaseSalary DECIMAL(18,4) NOT NULL,
  CurrencyCode CHAR(3) NOT NULL,
  PayFrequency TINYINT UNSIGNED NOT NULL DEFAULT 1,
  EmployeeInsuranceRate DECIMAL(9,6) NOT NULL,
  EmployerInsuranceRate DECIMAL(9,6) NOT NULL,
  IncomeTaxRate DECIMAL(9,6) NOT NULL,
  IsActive TINYINT(1) NOT NULL DEFAULT 1,
  CreatedAt DATETIME(6) NOT NULL,
  CONSTRAINT FK_EC_Emp FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);
CREATE TABLE IF NOT EXISTS PayrollRuns (
  PayrollRunID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  WebsiteID INT NOT NULL,
  RunNumber VARCHAR(40) NOT NULL,
  PeriodFrom DATE NOT NULL,
  PeriodTo DATE NOT NULL,
  Status TINYINT UNSIGNED NOT NULL,
  TotalGross DECIMAL(18,4) NOT NULL,
  TotalEmployeeInsurance DECIMAL(18,4) NOT NULL,
  TotalEmployerInsurance DECIMAL(18,4) NOT NULL,
  TotalIncomeTax DECIMAL(18,4) NOT NULL,
  TotalNet DECIMAL(18,4) NOT NULL,
  CurrencyCode CHAR(3) NOT NULL,
  Note VARCHAR(1000) NULL,
  CreatedByMemberID INT NULL,
  ApprovedByMemberID INT NULL,
  ApprovedAt DATETIME(6) NULL,
  PaidAt DATETIME(6) NULL,
  CreatedAt DATETIME(6) NOT NULL,
  CONSTRAINT FK_PR_Web FOREIGN KEY (WebsiteID) REFERENCES Websites(WebsiteID)
);
CREATE TABLE IF NOT EXISTS PayrollLines (
  PayrollLineID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  PayrollRunID INT NOT NULL,
  EmployeeID INT NOT NULL,
  Gross DECIMAL(18,4) NOT NULL,
  EmployeeInsurance DECIMAL(18,4) NOT NULL,
  EmployerInsurance DECIMAL(18,4) NOT NULL,
  IncomeTax DECIMAL(18,4) NOT NULL,
  Net DECIMAL(18,4) NOT NULL,
  EmployerCost DECIMAL(18,4) NOT NULL,
  Note VARCHAR(300) NULL,
  CONSTRAINT FK_PL_Run FOREIGN KEY (PayrollRunID) REFERENCES PayrollRuns(PayrollRunID),
  CONSTRAINT FK_PL_Emp FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);
");
}
protected override void Down(MigrationBuilder migrationBuilder) {
migrationBuilder.Sql("DROP TABLE IF EXISTS PayrollLines, PayrollRuns, EmployeeContracts, Employees, OrgUnits, StockDocumentHistories, StockDocumentLines, StockDocuments, WarehouseStocks, Warehouses;");
}
}}
