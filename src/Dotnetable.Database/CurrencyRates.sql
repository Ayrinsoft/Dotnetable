CREATE TABLE [dbo].[CurrencyRates] (
    [CurrencyRateID] INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]      INT             NOT NULL,
    [CurrencyCode]   CHAR (3)        NOT NULL,
    [USDToCurrency]  DECIMAL (18, 6) NOT NULL,
    [IsDefault]      BIT             NOT NULL,
    [LastUpdate]     DATETIME        NOT NULL,
    CONSTRAINT [PK_CurrencyRates] PRIMARY KEY CLUSTERED ([CurrencyRateID] ASC),
    CONSTRAINT [FK_CurrencyRates_Currencies] FOREIGN KEY ([CurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode]),
    CONSTRAINT [FK_CurrencyRates_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_CurrencyRates_CurrencyCode]
    ON [dbo].[CurrencyRates] ([CurrencyCode] ASC);

CREATE NONCLUSTERED INDEX [IX_CurrencyRates_WebsiteID]
    ON [dbo].[CurrencyRates] ([WebsiteID] ASC);
