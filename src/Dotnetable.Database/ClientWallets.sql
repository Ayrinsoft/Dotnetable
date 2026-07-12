CREATE TABLE [dbo].[ClientWallets] (
    [ClientWalletID]  INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT             NOT NULL,
    [WebsiteClientID] INT             NOT NULL,
    [BalanceUsd]      DECIMAL (18, 4) CONSTRAINT [DF_ClientWallets_BalanceUsd] DEFAULT ((0)) NOT NULL,
    [IsActive]        BIT             CONSTRAINT [DF_ClientWallets_IsActive] DEFAULT ((1)) NOT NULL,
    [RowVersion]      ROWVERSION      NOT NULL,
    [CreatedAt]       DATETIME2 (0)   CONSTRAINT [DF_ClientWallets_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_ClientWallets] PRIMARY KEY CLUSTERED ([ClientWalletID] ASC),
    CONSTRAINT [UQ_ClientWallets_WebsiteClientID] UNIQUE ([WebsiteClientID]),
    CONSTRAINT [FK_ClientWallets_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_ClientWallets_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID])
);

-- One wallet per WebsiteClient (already site-scoped). BalanceUsd is a cached total kept in
-- sync with ClientWalletTransactions inside the same transaction (RowVersion guards concurrent
-- spend/withdraw races) - it is never edited directly, only via a new ledger row.
GO

CREATE NONCLUSTERED INDEX [IX_ClientWallets_WebsiteID]
    ON [dbo].[ClientWallets] ([WebsiteID] ASC);
