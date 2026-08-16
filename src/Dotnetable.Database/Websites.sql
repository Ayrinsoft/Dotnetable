CREATE TABLE [dbo].[Websites] (
    [WebsiteID]                     INT              IDENTITY (1, 1) NOT NULL,
    [TradeName]                     NVARCHAR (32)    NOT NULL,
    [WebsiteAddress]                VARCHAR (60)     NOT NULL,
    [AuthCode]                      UNIQUEIDENTIFIER NOT NULL,
    [Active]                        BIT              NOT NULL,
    [Manager]                       NVARCHAR (30)    NOT NULL,
    [Mobile]                        VARCHAR (15)     NOT NULL,
    [Email]                         VARCHAR (60)     NOT NULL,
    [RegisterDate]                  DATE             NOT NULL,
    [AllowAllIP]                    BIT              NOT NULL,
    [DefaultLanguageCode]           CHAR (2)         NOT NULL,
    [WebsiteType]                   TINYINT          NOT NULL,
    [IsHub]                         BIT              NOT NULL,
    [BrandName]                     NVARCHAR (60)    NOT NULL,
    [LogoFileID]                    INT              NULL,
    [FaveIconFileID]                INT              NULL,
    [DefaultCurrencyCode]           CHAR (3)         NOT NULL,
    [StorePricesInUsd]              BIT              DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [TaxEnabled]                    BIT              DEFAULT (CONVERT([bit],(1))) NOT NULL,
    [PricesIncludeTax]              BIT              NOT NULL,
    [TaxOnShipping]                 BIT              NOT NULL,
    [TaxCountryID]                  INT              NULL,
    [SellerLegalName]               NVARCHAR (200)   NULL,
    [SellerTaxId]                   NVARCHAR (50)    NULL,
    [SellerEconomicCode]            NVARCHAR (50)    NULL,
    [SellerVatNumber]               NVARCHAR (50)    NULL,
    [SellerRegistrationNumber]      NVARCHAR (50)    NULL,
    [AllowCashOnDelivery]           BIT              DEFAULT (CONVERT([bit],(1))) NOT NULL,
    [ReportOfflineOrdersToTax]      BIT              DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [FreeShippingMinOrderAmount]    DECIMAL (18, 4)  NOT NULL,
    [FreeShippingMinOrderAmountUsd] DECIMAL (18, 4)  NOT NULL,
    [ProductCodePrefix]             NVARCHAR (3)     DEFAULT (N'DN') NOT NULL,
    [FiscalPeriodCadence]           TINYINT          DEFAULT (CONVERT([tinyint],(3))) NOT NULL,
    [FiscalYearStartMonth]          TINYINT          DEFAULT (CONVERT([tinyint],(1))) NOT NULL,
    [FiscalWeekStartDay]            TINYINT          DEFAULT (CONVERT([tinyint],(1))) NOT NULL,
    [FiscalCloseDueDays]            INT              DEFAULT ((5)) NOT NULL,
    [ReturnsEnabled]                BIT              DEFAULT (CONVERT([bit],(1))) NOT NULL,
    [ReturnWindowDays]              INT              DEFAULT ((7)) NOT NULL,
    [ReturnWindowFrom]              TINYINT          DEFAULT (CONVERT([tinyint],(0))) NOT NULL,
    CONSTRAINT [PK_Websites] PRIMARY KEY CLUSTERED ([WebsiteID] ASC),
    CONSTRAINT [FK_Websites_Currencies] FOREIGN KEY ([DefaultCurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode]),
    CONSTRAINT [FK_Websites_FileRecord1] FOREIGN KEY ([FaveIconFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_Websites_FileRecords] FOREIGN KEY ([LogoFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
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

GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'When true, also persist USD dual columns; default site currency is always operational authority.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Websites', @level2type = N'COLUMN', @level2name = N'StorePricesInUsd';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'1–3 letter product code prefix; codes are {prefix}-{ProductID}.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Websites', @level2type = N'COLUMN', @level2name = N'ProductCodePrefix';

