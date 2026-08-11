CREATE TABLE [dbo].[WebsiteWalletCurrencies] (
    [WebsiteWalletCurrencyID] INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]               INT           NOT NULL,
    [CurrencyCode]            CHAR (3)      NOT NULL,
    [IsDefault]               BIT           NOT NULL CONSTRAINT [DF_WebsiteWalletCurrencies_IsDefault] DEFAULT ((0)),
    [IsActive]                BIT           NOT NULL CONSTRAINT [DF_WebsiteWalletCurrencies_IsActive] DEFAULT ((1)),
    [CreatedAt]               DATETIME2 (0) NOT NULL,
    CONSTRAINT [PK_WebsiteWalletCurrencies] PRIMARY KEY CLUSTERED ([WebsiteWalletCurrencyID] ASC),
    CONSTRAINT [UQ_WebsiteWalletCurrencies_Website_Currency] UNIQUE ([WebsiteID], [CurrencyCode]),
    CONSTRAINT [FK_WebsiteWalletCurrencies_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_WebsiteWalletCurrencies_Currencies] FOREIGN KEY ([CurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode])
);

-- Currencies customers may open separate wallet accounts for.
-- Exactly one IsDefault=1 row per website (site DefaultCurrencyCode).
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteWalletCurrencies_WebsiteID]
    ON [dbo].[WebsiteWalletCurrencies] ([WebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteWalletCurrencies_CurrencyCode]
    ON [dbo].[WebsiteWalletCurrencies] ([CurrencyCode] ASC);
