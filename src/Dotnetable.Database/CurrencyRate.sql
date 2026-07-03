CREATE TABLE [dbo].[CurrencyRate] (
    [CurrencyRateID] INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]      INT             NOT NULL,
    [CurrencyCode]   CHAR (3)        NOT NULL,
    [USDToCurrency]  DECIMAL (18, 6) NOT NULL,
    [IsDefault]      BIT             NOT NULL,
    [LastUpdate]     DATETIME        NOT NULL,
    CONSTRAINT [PK_CurrencyRate] PRIMARY KEY CLUSTERED ([CurrencyRateID] ASC),
    CONSTRAINT [FK_CurrencyRate_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

