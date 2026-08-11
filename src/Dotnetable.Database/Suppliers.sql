CREATE TABLE [dbo].[Suppliers] (
    [SupplierID]              INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]               INT             NOT NULL,
    [Name]                    NVARCHAR (200)  NOT NULL,
    [Phone]                   NVARCHAR (20)   NULL,
    [IsActive]                BIT             NOT NULL,
    [AddressLine]             NVARCHAR (500)  NULL,
    [BankAccountNumber]       NVARCHAR (50)   NULL,
    [BankIban]                NVARCHAR (34)   NULL,
    [BankName]                NVARCHAR (100)  NULL,
    [CityName]                NVARCHAR (100)  NULL,
    [CountryID]               INT             NULL,
    [CreatedAt]               DATETIME        DEFAULT (getutcdate()) NOT NULL,
    [DefaultCurrencyCode]     CHAR (3)        NULL,
    [EconomicCode]            NVARCHAR (50)   NULL,
    [Email]                   NVARCHAR (120)  NULL,
    [IsVatRegistered]         BIT             DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [LegalName]               NVARCHAR (200)  NULL,
    [LinkedVendorID]          INT             NULL,
    [LinkedWebsiteID]         INT             NULL,
    [Notes]                   NVARCHAR (1000) NULL,
    [PostalCode]              NVARCHAR (20)   NULL,
    [RegistrationNumber]      NVARCHAR (50)   NULL,
    [SupplierType]            TINYINT         DEFAULT (CONVERT([tinyint],(0))) NOT NULL,
    [TaxIdentificationNumber] NVARCHAR (50)   NULL,
    [VatNumber]               NVARCHAR (50)   NULL,
    CONSTRAINT [PK_Suppliers] PRIMARY KEY CLUSTERED ([SupplierID] ASC),
    CONSTRAINT [FK_Suppliers_Countries] FOREIGN KEY ([CountryID]) REFERENCES [dbo].[Countries] ([CountryID]),
    CONSTRAINT [FK_Suppliers_Currencies] FOREIGN KEY ([DefaultCurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode]),
    CONSTRAINT [FK_Suppliers_LinkedWebsites] FOREIGN KEY ([LinkedWebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_Suppliers_Vendors] FOREIGN KEY ([LinkedVendorID]) REFERENCES [dbo].[Vendors] ([VendorID]),
    CONSTRAINT [FK_Suppliers_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


GO

CREATE NONCLUSTERED INDEX [IX_Suppliers_WebsiteID]
    ON [dbo].[Suppliers] ([WebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Suppliers_CountryID]
    ON [dbo].[Suppliers] ([CountryID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Suppliers_LinkedWebsiteID]
    ON [dbo].[Suppliers] ([LinkedWebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Suppliers_LinkedVendorID]
    ON [dbo].[Suppliers] ([LinkedVendorID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Suppliers_DefaultCurrencyCode]
    ON [dbo].[Suppliers]([DefaultCurrencyCode] ASC);

