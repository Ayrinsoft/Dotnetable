CREATE TABLE [dbo].[ClientWalletTransactions] (
    [ClientWalletTransactionID] INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]                 INT             NOT NULL,
    [ClientWalletID]            INT             NOT NULL,
    [Type]                      TINYINT         NOT NULL,
    [AmountUsd]                 DECIMAL (18, 4) NOT NULL,
    [BalanceAfterUsd]           DECIMAL (18, 4) NOT NULL,
    [SourceType]                TINYINT         NULL,
    [SourceId]                  INT             NULL,
    [Note]                      NVARCHAR (500)  NULL,
    [CreatedByMemberID]         INT             NULL,
    [CreatedAt]                 DATETIME2 (0)   NOT NULL,
    [Amount]                    DECIMAL (18, 4) DEFAULT ((0.0)) NOT NULL,
    [BalanceAfter]              DECIMAL (18, 4) DEFAULT ((0.0)) NOT NULL,
    CONSTRAINT [PK_ClientWalletTransactions] PRIMARY KEY CLUSTERED ([ClientWalletTransactionID] ASC),
    CONSTRAINT [FK_ClientWalletTransactions_ClientWallets] FOREIGN KEY ([ClientWalletID]) REFERENCES [dbo].[ClientWallets] ([ClientWalletID]),
    CONSTRAINT [FK_ClientWalletTransactions_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_ClientWalletTransactions_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);



-- Append-only ledger in the wallet's CurrencyCode. Amount is signed (+credit / -debit).
-- Type: 1=RefundCredit, 2=PurchaseUse, 3=WithdrawalHold, 4=WithdrawalReversed, 5=AdminAdjustment, 6=AdminDeposit.
-- AmountUsd/BalanceAfterUsd are optional reporting mirrors only.
GO

CREATE NONCLUSTERED INDEX [IX_ClientWalletTransactions_ClientWalletID]
    ON [dbo].[ClientWalletTransactions] ([ClientWalletID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ClientWalletTransactions_CreatedByMemberID]
    ON [dbo].[ClientWalletTransactions] ([CreatedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ClientWalletTransactions_WebsiteID]
    ON [dbo].[ClientWalletTransactions] ([WebsiteID] ASC);
