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
    [CreatedAt]                 DATETIME2 (0)   CONSTRAINT [DF_ClientWalletTransactions_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_ClientWalletTransactions] PRIMARY KEY CLUSTERED ([ClientWalletTransactionID] ASC),
    CONSTRAINT [FK_ClientWalletTransactions_ClientWallets] FOREIGN KEY ([ClientWalletID]) REFERENCES [dbo].[ClientWallets] ([ClientWalletID]),
    CONSTRAINT [FK_ClientWalletTransactions_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_ClientWalletTransactions_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

-- Append-only ledger; ClientWallets.BalanceUsd is always the running sum of AmountUsd here.
-- AmountUsd is signed (+credit / -debit). Type: 1=RefundCredit, 2=PurchaseUse, 3=WithdrawalHold,
-- 4=WithdrawalReversed, 5=AdminAdjustment.
-- SourceType/SourceId is a polymorphic pointer (unenforced, like JournalEntries.SourceType/SourceId):
-- 1=PaymentRefund, 2=Payment, 3=ClientWalletWithdrawal, 4=AdminManual.
GO

CREATE NONCLUSTERED INDEX [IX_ClientWalletTransactions_ClientWalletID]
    ON [dbo].[ClientWalletTransactions] ([ClientWalletID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ClientWalletTransactions_CreatedByMemberID]
    ON [dbo].[ClientWalletTransactions] ([CreatedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ClientWalletTransactions_WebsiteID]
    ON [dbo].[ClientWalletTransactions] ([WebsiteID] ASC);
