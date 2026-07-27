-- Supplier tax identity + website tax settings + order/settlement tax fields + tax rate metadata
-- Safe additive migration for SQL Server SSDT / manual apply.

-- Websites: tax behavior + seller registration
IF COL_LENGTH('dbo.Websites', 'TaxEnabled') IS NULL
BEGIN
    ALTER TABLE [dbo].[Websites] ADD
        [TaxEnabled] BIT NOT NULL CONSTRAINT [DF_Websites_TaxEnabled] DEFAULT (1),
        [PricesIncludeTax] BIT NOT NULL CONSTRAINT [DF_Websites_PricesIncludeTax] DEFAULT (0),
        [TaxOnShipping] BIT NOT NULL CONSTRAINT [DF_Websites_TaxOnShipping] DEFAULT (0),
        [TaxCountryID] INT NULL,
        [SellerLegalName] NVARCHAR(200) NULL,
        [SellerTaxId] NVARCHAR(50) NULL,
        [SellerEconomicCode] NVARCHAR(50) NULL,
        [SellerVatNumber] NVARCHAR(50) NULL,
        [SellerRegistrationNumber] NVARCHAR(50) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Websites_TaxCountries')
BEGIN
    ALTER TABLE [dbo].[Websites] WITH CHECK
        ADD CONSTRAINT [FK_Websites_TaxCountries] FOREIGN KEY ([TaxCountryID]) REFERENCES [dbo].[Countries] ([CountryID]);
END
GO

-- Suppliers: full tax / bank identity
IF COL_LENGTH('dbo.Suppliers', 'LegalName') IS NULL
BEGIN
    ALTER TABLE [dbo].[Suppliers] ADD
        [LegalName] NVARCHAR(200) NULL,
        [Email] NVARCHAR(120) NULL,
        [AddressLine] NVARCHAR(500) NULL,
        [CityName] NVARCHAR(100) NULL,
        [PostalCode] NVARCHAR(20) NULL,
        [CountryID] INT NULL,
        [TaxIdentificationNumber] NVARCHAR(50) NULL,
        [EconomicCode] NVARCHAR(50) NULL,
        [VatNumber] NVARCHAR(50) NULL,
        [RegistrationNumber] NVARCHAR(50) NULL,
        [IsVatRegistered] BIT NOT NULL CONSTRAINT [DF_Suppliers_IsVatRegistered] DEFAULT (0),
        [BankName] NVARCHAR(100) NULL,
        [BankIban] NVARCHAR(34) NULL,
        [BankAccountNumber] NVARCHAR(50) NULL,
        [DefaultCurrencyCode] CHAR(3) NULL,
        [SupplierType] TINYINT NOT NULL CONSTRAINT [DF_Suppliers_SupplierType] DEFAULT (0),
        [LinkedWebsiteID] INT NULL,
        [LinkedVendorID] INT NULL,
        [Notes] NVARCHAR(1000) NULL,
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_Suppliers_CreatedAt] DEFAULT (GETUTCDATE());
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Suppliers_CountryID' AND object_id = OBJECT_ID(N'dbo.Suppliers'))
    CREATE NONCLUSTERED INDEX [IX_Suppliers_CountryID] ON [dbo].[Suppliers] ([CountryID] ASC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Suppliers_LinkedWebsiteID' AND object_id = OBJECT_ID(N'dbo.Suppliers'))
    CREATE NONCLUSTERED INDEX [IX_Suppliers_LinkedWebsiteID] ON [dbo].[Suppliers] ([LinkedWebsiteID] ASC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Suppliers_LinkedVendorID' AND object_id = OBJECT_ID(N'dbo.Suppliers'))
    CREATE NONCLUSTERED INDEX [IX_Suppliers_LinkedVendorID] ON [dbo].[Suppliers] ([LinkedVendorID] ASC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Suppliers_Countries')
    ALTER TABLE [dbo].[Suppliers] WITH CHECK ADD CONSTRAINT [FK_Suppliers_Countries] FOREIGN KEY ([CountryID]) REFERENCES [dbo].[Countries] ([CountryID]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Suppliers_Currencies')
    ALTER TABLE [dbo].[Suppliers] WITH CHECK ADD CONSTRAINT [FK_Suppliers_Currencies] FOREIGN KEY ([DefaultCurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Suppliers_LinkedWebsites')
    ALTER TABLE [dbo].[Suppliers] WITH CHECK ADD CONSTRAINT [FK_Suppliers_LinkedWebsites] FOREIGN KEY ([LinkedWebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Suppliers_Vendors')
    ALTER TABLE [dbo].[Suppliers] WITH CHECK ADD CONSTRAINT [FK_Suppliers_Vendors] FOREIGN KEY ([LinkedVendorID]) REFERENCES [dbo].[Vendors] ([VendorID]);
GO

-- TaxRates metadata
IF COL_LENGTH('dbo.TaxRates', 'TaxCode') IS NULL
BEGIN
    ALTER TABLE [dbo].[TaxRates] ADD
        [TaxCode] NVARCHAR(30) NULL,
        [TaxKind] TINYINT NOT NULL CONSTRAINT [DF_TaxRates_TaxKind] DEFAULT (0),
        [ApplyToShipping] BIT NOT NULL CONSTRAINT [DF_TaxRates_ApplyToShipping] DEFAULT (0);
END
GO

-- Orders: tax snapshot
IF COL_LENGTH('dbo.Orders', 'PricesIncludeTax') IS NULL
BEGIN
    ALTER TABLE [dbo].[Orders] ADD
        [PricesIncludeTax] BIT NOT NULL CONSTRAINT [DF_Orders_PricesIncludeTax] DEFAULT (0),
        [TaxBreakdownJson] NVARCHAR(4000) NULL;
END
GO

-- Settlements: net / tax / gross
IF COL_LENGTH('dbo.Settlements', 'NetAmount') IS NULL
BEGIN
    ALTER TABLE [dbo].[Settlements] ADD
        [NetAmount] DECIMAL(18, 4) NOT NULL CONSTRAINT [DF_Settlements_NetAmount] DEFAULT (0),
        [TaxAmount] DECIMAL(18, 4) NOT NULL CONSTRAINT [DF_Settlements_TaxAmount] DEFAULT (0),
        [TaxRateSnapshot] DECIMAL(9, 6) NULL;

    -- Backfill: treat historical TotalAmount as net (and gross) with zero tax.
    EXEC(N'UPDATE [dbo].[Settlements] SET [NetAmount] = [TotalAmount] WHERE [NetAmount] = 0 AND [TotalAmount] <> 0');
END
GO
