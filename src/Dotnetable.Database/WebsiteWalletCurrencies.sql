CREATE TABLE [dbo].[WebsiteWalletCurrencies] (
    [WebsiteWalletCurrencyID] INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]               INT           NOT NULL,
    [CurrencyCode]            CHAR (3)      NOT NULL,
    [IsDefault]               BIT           NOT NULL,
    [IsActive]                BIT           NOT NULL,
    [CreatedAt]               DATETIME2 (0) NOT NULL,
    CONSTRAINT [PK_WebsiteWalletCurrencies] PRIMARY KEY CLUSTERED ([WebsiteWalletCurrencyID] ASC),
    CONSTRAINT [FK_WebsiteWalletCurrencies_Currencies] FOREIGN KEY ([CurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode]),
    CONSTRAINT [FK_WebsiteWalletCurrencies_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);



-- Currencies customers may open separate wallet accounts for.
-- Exactly one IsDefault=1 row per website (site DefaultCurrencyCode).
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteWalletCurrencies_WebsiteID]
    ON [dbo].[WebsiteWalletCurrencies] ([WebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteWalletCurrencies_CurrencyCode]
    ON [dbo].[WebsiteWalletCurrencies] ([CurrencyCode] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_WebsiteWalletCurrencies_Website_Currency]
    ON [dbo].[WebsiteWalletCurrencies]([WebsiteID] ASC, [CurrencyCode] ASC);

