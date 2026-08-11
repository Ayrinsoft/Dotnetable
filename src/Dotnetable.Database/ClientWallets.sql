CREATE TABLE [dbo].[ClientWallets] (
    [ClientWalletID]  INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT             NOT NULL,
    [WebsiteClientID] INT             NOT NULL,
    [CurrencyCode]    CHAR (3)        NOT NULL,
    [Balance]         DECIMAL (18, 4) NOT NULL CONSTRAINT [DF_ClientWallets_Balance] DEFAULT ((0)),
    [BalanceUsd]      DECIMAL (18, 4) NOT NULL CONSTRAINT [DF_ClientWallets_BalanceUsd] DEFAULT ((0)),
    [IsActive]        BIT             NOT NULL,
    [RowVersion]      ROWVERSION      NOT NULL,
    [CreatedAt]       DATETIME2 (0)   NOT NULL,
    CONSTRAINT [PK_ClientWallets] PRIMARY KEY CLUSTERED ([ClientWalletID] ASC),
    CONSTRAINT [UQ_ClientWallets_Client_Currency] UNIQUE ([WebsiteClientID], [CurrencyCode]),
    CONSTRAINT [FK_ClientWallets_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_ClientWallets_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID]),
    CONSTRAINT [FK_ClientWallets_Currencies] FOREIGN KEY ([CurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode])
);

-- One wallet ledger per customer per currency. Balance is the authority in CurrencyCode only.
-- BalanceUsd is optional reporting mirror (not used for spend checks).
-- RowVersion guards concurrent spend/withdraw; balance changes only via ClientWalletTransactions.
GO

CREATE NONCLUSTERED INDEX [IX_ClientWallets_WebsiteID]
    ON [dbo].[ClientWallets] ([WebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ClientWallets_CurrencyCode]
    ON [dbo].[ClientWallets] ([CurrencyCode] ASC);
