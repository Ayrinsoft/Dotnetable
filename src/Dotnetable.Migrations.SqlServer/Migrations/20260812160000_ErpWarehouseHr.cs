using System;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260812160000_ErpWarehouseHr")]
    public class ErpWarehouseHr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Warehouses]', N'U') IS NULL
CREATE TABLE [dbo].[Warehouses](
  [WarehouseID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [WebsiteID] INT NOT NULL,
  [Code] NVARCHAR(20) NOT NULL,
  [Name] NVARCHAR(200) NOT NULL,
  [Address] NVARCHAR(500) NULL,
  [IsDefault] BIT NOT NULL CONSTRAINT DF_Wh_IsDefault DEFAULT(0),
  [IsActive] BIT NOT NULL CONSTRAINT DF_Wh_IsActive DEFAULT(1),
  [CreatedAt] DATETIME2 NOT NULL,
  CONSTRAINT FK_Warehouses_Websites FOREIGN KEY([WebsiteID]) REFERENCES [dbo].[Websites]([WebsiteID])
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Warehouses_Website_Code')
  CREATE UNIQUE INDEX IX_Warehouses_Website_Code ON [dbo].[Warehouses]([WebsiteID],[Code]);

IF OBJECT_ID(N'[dbo].[WarehouseStocks]', N'U') IS NULL
CREATE TABLE [dbo].[WarehouseStocks](
  [WarehouseStockID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [WarehouseID] INT NOT NULL,
  [ProductVariantID] INT NOT NULL,
  [QuantityOnHand] INT NOT NULL,
  [QuantityReserved] INT NOT NULL CONSTRAINT DF_WS_Res DEFAULT(0),
  [RowVersion] ROWVERSION NOT NULL,
  CONSTRAINT FK_WS_Wh FOREIGN KEY([WarehouseID]) REFERENCES [dbo].[Warehouses]([WarehouseID]),
  CONSTRAINT FK_WS_Var FOREIGN KEY([ProductVariantID]) REFERENCES [dbo].[ProductVariants]([ProductVariantID])
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_WarehouseStocks_Warehouse_Variant')
  CREATE UNIQUE INDEX IX_WarehouseStocks_Warehouse_Variant ON [dbo].[WarehouseStocks]([WarehouseID],[ProductVariantID]);

IF OBJECT_ID(N'[dbo].[StockDocuments]', N'U') IS NULL
CREATE TABLE [dbo].[StockDocuments](
  [StockDocumentID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [WebsiteID] INT NOT NULL,
  [DocumentNumber] NVARCHAR(40) NOT NULL,
  [DocumentType] TINYINT NOT NULL,
  [Status] TINYINT NOT NULL,
  [FromWarehouseID] INT NULL,
  [ToWarehouseID] INT NULL,
  [SupplierID] INT NULL,
  [OrderID] INT NULL,
  [Note] NVARCHAR(1000) NULL,
  [RequestedByMemberID] INT NULL,
  [ApprovedByMemberID] INT NULL,
  [PostedByMemberID] INT NULL,
  [SubmittedAt] DATETIME2 NULL,
  [ApprovedAt] DATETIME2 NULL,
  [PostedAt] DATETIME2 NULL,
  [CreatedAt] DATETIME2 NOT NULL,
  CONSTRAINT FK_SD_Web FOREIGN KEY([WebsiteID]) REFERENCES [dbo].[Websites]([WebsiteID]),
  CONSTRAINT FK_SD_From FOREIGN KEY([FromWarehouseID]) REFERENCES [dbo].[Warehouses]([WarehouseID]),
  CONSTRAINT FK_SD_To FOREIGN KEY([ToWarehouseID]) REFERENCES [dbo].[Warehouses]([WarehouseID]),
  CONSTRAINT FK_SD_Sup FOREIGN KEY([SupplierID]) REFERENCES [dbo].[Suppliers]([SupplierID]),
  CONSTRAINT FK_SD_Ord FOREIGN KEY([OrderID]) REFERENCES [dbo].[Orders]([OrderID]),
  CONSTRAINT FK_SD_Req FOREIGN KEY([RequestedByMemberID]) REFERENCES [dbo].[Members]([MemberID]),
  CONSTRAINT FK_SD_App FOREIGN KEY([ApprovedByMemberID]) REFERENCES [dbo].[Members]([MemberID]),
  CONSTRAINT FK_SD_Post FOREIGN KEY([PostedByMemberID]) REFERENCES [dbo].[Members]([MemberID])
);

IF OBJECT_ID(N'[dbo].[StockDocumentLines]', N'U') IS NULL
CREATE TABLE [dbo].[StockDocumentLines](
  [StockDocumentLineID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [StockDocumentID] INT NOT NULL,
  [ProductVariantID] INT NOT NULL,
  [Quantity] INT NOT NULL,
  [UnitCost] DECIMAL(18,4) NOT NULL,
  [UnitCostUsd] DECIMAL(18,4) NOT NULL,
  [Note] NVARCHAR(300) NULL,
  CONSTRAINT FK_SDL_Doc FOREIGN KEY([StockDocumentID]) REFERENCES [dbo].[StockDocuments]([StockDocumentID]),
  CONSTRAINT FK_SDL_Var FOREIGN KEY([ProductVariantID]) REFERENCES [dbo].[ProductVariants]([ProductVariantID])
);

IF OBJECT_ID(N'[dbo].[StockDocumentHistories]', N'U') IS NULL
CREATE TABLE [dbo].[StockDocumentHistories](
  [StockDocumentHistoryID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [StockDocumentID] INT NOT NULL,
  [FromStatus] TINYINT NOT NULL,
  [ToStatus] TINYINT NOT NULL,
  [Note] NVARCHAR(500) NULL,
  [CreatedByMemberID] INT NULL,
  [CreatedAt] DATETIME2 NOT NULL,
  CONSTRAINT FK_SDH_Doc FOREIGN KEY([StockDocumentID]) REFERENCES [dbo].[StockDocuments]([StockDocumentID]),
  CONSTRAINT FK_SDH_Mem FOREIGN KEY([CreatedByMemberID]) REFERENCES [dbo].[Members]([MemberID])
);

IF OBJECT_ID(N'[dbo].[OrgUnits]', N'U') IS NULL
CREATE TABLE [dbo].[OrgUnits](
  [OrgUnitID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [WebsiteID] INT NOT NULL,
  [ParentOrgUnitID] INT NULL,
  [Code] NVARCHAR(20) NOT NULL,
  [Name] NVARCHAR(200) NOT NULL,
  [SortOrder] INT NOT NULL CONSTRAINT DF_OU_Sort DEFAULT(0),
  [IsActive] BIT NOT NULL CONSTRAINT DF_OU_Act DEFAULT(1),
  CONSTRAINT FK_OU_Web FOREIGN KEY([WebsiteID]) REFERENCES [dbo].[Websites]([WebsiteID]),
  CONSTRAINT FK_OU_Par FOREIGN KEY([ParentOrgUnitID]) REFERENCES [dbo].[OrgUnits]([OrgUnitID])
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_OrgUnits_Website_Code')
  CREATE UNIQUE INDEX IX_OrgUnits_Website_Code ON [dbo].[OrgUnits]([WebsiteID],[Code]);

IF OBJECT_ID(N'[dbo].[Employees]', N'U') IS NULL
CREATE TABLE [dbo].[Employees](
  [EmployeeID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [WebsiteID] INT NOT NULL,
  [EmployeeCode] NVARCHAR(30) NOT NULL,
  [GivenName] NVARCHAR(100) NOT NULL,
  [Surname] NVARCHAR(100) NOT NULL,
  [NationalId] NVARCHAR(50) NULL,
  [Email] NVARCHAR(200) NULL,
  [Phone] NVARCHAR(50) NULL,
  [OrgUnitID] INT NULL,
  [JobTitle] NVARCHAR(150) NULL,
  [MemberID] INT NULL,
  [Status] TINYINT NOT NULL CONSTRAINT DF_Emp_St DEFAULT(1),
  [HireDate] DATE NOT NULL,
  [TerminationDate] DATE NULL,
  [CreatedAt] DATETIME2 NOT NULL,
  CONSTRAINT FK_Emp_Web FOREIGN KEY([WebsiteID]) REFERENCES [dbo].[Websites]([WebsiteID]),
  CONSTRAINT FK_Emp_OU FOREIGN KEY([OrgUnitID]) REFERENCES [dbo].[OrgUnits]([OrgUnitID]),
  CONSTRAINT FK_Emp_Mem FOREIGN KEY([MemberID]) REFERENCES [dbo].[Members]([MemberID])
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Employees_Website_Code')
  CREATE UNIQUE INDEX IX_Employees_Website_Code ON [dbo].[Employees]([WebsiteID],[EmployeeCode]);

IF OBJECT_ID(N'[dbo].[EmployeeContracts]', N'U') IS NULL
CREATE TABLE [dbo].[EmployeeContracts](
  [EmployeeContractID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [EmployeeID] INT NOT NULL,
  [EffectiveFrom] DATE NOT NULL,
  [EffectiveTo] DATE NULL,
  [BaseSalary] DECIMAL(18,4) NOT NULL,
  [CurrencyCode] CHAR(3) NOT NULL,
  [PayFrequency] TINYINT NOT NULL CONSTRAINT DF_EC_Freq DEFAULT(1),
  [EmployeeInsuranceRate] DECIMAL(9,6) NOT NULL,
  [EmployerInsuranceRate] DECIMAL(9,6) NOT NULL,
  [IncomeTaxRate] DECIMAL(9,6) NOT NULL,
  [IsActive] BIT NOT NULL CONSTRAINT DF_EC_Act DEFAULT(1),
  [CreatedAt] DATETIME2 NOT NULL,
  CONSTRAINT FK_EC_Emp FOREIGN KEY([EmployeeID]) REFERENCES [dbo].[Employees]([EmployeeID]),
  CONSTRAINT FK_EC_Cur FOREIGN KEY([CurrencyCode]) REFERENCES [dbo].[Currencies]([CurrencyCode])
);

IF OBJECT_ID(N'[dbo].[PayrollRuns]', N'U') IS NULL
CREATE TABLE [dbo].[PayrollRuns](
  [PayrollRunID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [WebsiteID] INT NOT NULL,
  [RunNumber] NVARCHAR(40) NOT NULL,
  [PeriodFrom] DATE NOT NULL,
  [PeriodTo] DATE NOT NULL,
  [Status] TINYINT NOT NULL,
  [TotalGross] DECIMAL(18,4) NOT NULL,
  [TotalEmployeeInsurance] DECIMAL(18,4) NOT NULL,
  [TotalEmployerInsurance] DECIMAL(18,4) NOT NULL,
  [TotalIncomeTax] DECIMAL(18,4) NOT NULL,
  [TotalNet] DECIMAL(18,4) NOT NULL,
  [CurrencyCode] CHAR(3) NOT NULL,
  [Note] NVARCHAR(1000) NULL,
  [CreatedByMemberID] INT NULL,
  [ApprovedByMemberID] INT NULL,
  [ApprovedAt] DATETIME2 NULL,
  [PaidAt] DATETIME2 NULL,
  [CreatedAt] DATETIME2 NOT NULL,
  CONSTRAINT FK_PR_Web FOREIGN KEY([WebsiteID]) REFERENCES [dbo].[Websites]([WebsiteID]),
  CONSTRAINT FK_PR_Cur FOREIGN KEY([CurrencyCode]) REFERENCES [dbo].[Currencies]([CurrencyCode]),
  CONSTRAINT FK_PR_Cr FOREIGN KEY([CreatedByMemberID]) REFERENCES [dbo].[Members]([MemberID]),
  CONSTRAINT FK_PR_Ap FOREIGN KEY([ApprovedByMemberID]) REFERENCES [dbo].[Members]([MemberID])
);

IF OBJECT_ID(N'[dbo].[PayrollLines]', N'U') IS NULL
CREATE TABLE [dbo].[PayrollLines](
  [PayrollLineID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [PayrollRunID] INT NOT NULL,
  [EmployeeID] INT NOT NULL,
  [Gross] DECIMAL(18,4) NOT NULL,
  [EmployeeInsurance] DECIMAL(18,4) NOT NULL,
  [EmployerInsurance] DECIMAL(18,4) NOT NULL,
  [IncomeTax] DECIMAL(18,4) NOT NULL,
  [Net] DECIMAL(18,4) NOT NULL,
  [EmployerCost] DECIMAL(18,4) NOT NULL,
  [Note] NVARCHAR(300) NULL,
  CONSTRAINT FK_PL_Run FOREIGN KEY([PayrollRunID]) REFERENCES [dbo].[PayrollRuns]([PayrollRunID]),
  CONSTRAINT FK_PL_Emp FOREIGN KEY([EmployeeID]) REFERENCES [dbo].[Employees]([EmployeeID])
);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TABLE IF EXISTS [dbo].[PayrollLines];
DROP TABLE IF EXISTS [dbo].[PayrollRuns];
DROP TABLE IF EXISTS [dbo].[EmployeeContracts];
DROP TABLE IF EXISTS [dbo].[Employees];
DROP TABLE IF EXISTS [dbo].[OrgUnits];
DROP TABLE IF EXISTS [dbo].[StockDocumentHistories];
DROP TABLE IF EXISTS [dbo].[StockDocumentLines];
DROP TABLE IF EXISTS [dbo].[StockDocuments];
DROP TABLE IF EXISTS [dbo].[WarehouseStocks];
DROP TABLE IF EXISTS [dbo].[Warehouses];
");
        }
    }
}
