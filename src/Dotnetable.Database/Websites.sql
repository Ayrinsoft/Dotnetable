CREATE TABLE [dbo].[Websites] (
    [WebsiteID]                      INT              IDENTITY (1, 1) NOT NULL,
    [TradeName]                      NVARCHAR (32)    NOT NULL,
    [WebsiteAddress]                 VARCHAR (60)     NOT NULL,
    [AuthCode]                       UNIQUEIDENTIFIER NOT NULL,
    [Active]                         BIT              NOT NULL,
    [Manager]                        NVARCHAR (30)    NOT NULL,
    [Mobile]                         VARCHAR (15)     NOT NULL,
    [Email]                          VARCHAR (60)     NOT NULL,
    [RegisterDate]                   DATE             NOT NULL,
    [AllowAllIP]                     BIT              NOT NULL,
    [DefaultLanguageCode]            CHAR (2)         NOT NULL,
    [WebsiteType]                    TINYINT          NOT NULL,
    [IsHub]                          BIT              NOT NULL,
    [BrandName]                      NVARCHAR (60)    NOT NULL,
    [LogoFileID]                     INT              NULL,
    [FaveIconFileID]                 INT              NULL,
    [DefaultCurrencyCode]            CHAR (3)         NOT NULL,
    [StorePricesInUsd]               BIT              NOT NULL CONSTRAINT [DF_Websites_StorePricesInUsd] DEFAULT ((0)),
    [TaxEnabled]                     BIT              NOT NULL CONSTRAINT [DF_Websites_TaxEnabled] DEFAULT ((1)),
    [PricesIncludeTax]               BIT              NOT NULL CONSTRAINT [DF_Websites_PricesIncludeTax] DEFAULT ((0)),
    [TaxOnShipping]                  BIT              NOT NULL CONSTRAINT [DF_Websites_TaxOnShipping] DEFAULT ((0)),
    [TaxCountryID]                   INT              NULL,
    [SellerLegalName]                NVARCHAR (200)   NULL,
    [SellerTaxId]                    NVARCHAR (50)    NULL,
    [SellerEconomicCode]             NVARCHAR (50)    NULL,
    [SellerVatNumber]                NVARCHAR (50)    NULL,
    [SellerRegistrationNumber]       NVARCHAR (50)    NULL,
    [AllowCashOnDelivery]            BIT              NOT NULL CONSTRAINT [DF_Websites_AllowCashOnDelivery] DEFAULT ((1)),
    [ReportOfflineOrdersToTax]       BIT              NOT NULL CONSTRAINT [DF_Websites_ReportOfflineOrdersToTax] DEFAULT ((0)),
    [FreeShippingMinOrderAmount]     DECIMAL (18, 4)  NOT NULL CONSTRAINT [DF_Websites_FreeShippingMinOrderAmount] DEFAULT ((0)),
    [FreeShippingMinOrderAmountUsd]  DECIMAL (18, 4)  NOT NULL CONSTRAINT [DF_Websites_FreeShippingMinOrderAmountUsd] DEFAULT ((0)),
    CONSTRAINT [PK_Websites] PRIMARY KEY CLUSTERED ([WebsiteID] ASC),
    CONSTRAINT [FK_Websites_Currencies] FOREIGN KEY ([DefaultCurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode]),
    CONSTRAINT [FK_Websites_FileRecords] FOREIGN KEY ([LogoFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_Websites_FileRecord1] FOREIGN KEY ([FaveIconFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_Websites_TaxCountries] FOREIGN KEY ([TaxCountryID]) REFERENCES [dbo].[Countries] ([CountryID])
);




GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'show in title of pages', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Websites', @level2type = N'COLUMN', @level2name = N'BrandName';
GO

CREATE NONCLUSTERED INDEX [IX_Websites_DefaultCurrencyCode]
    ON [dbo].[Websites] ([DefaultCurrencyCode] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Websites_FaveIconFileID]
    ON [dbo].[Websites] ([FaveIconFileID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Websites_LogoFileID]
    ON [dbo].[Websites] ([LogoFileID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Websites_TaxCountryID]
    ON [dbo].[Websites] ([TaxCountryID] ASC);
