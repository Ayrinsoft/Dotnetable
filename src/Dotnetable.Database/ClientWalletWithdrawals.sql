CREATE TABLE [dbo].[ClientWalletWithdrawals] (
    [ClientWalletWithdrawalID] INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]                INT             NOT NULL,
    [WebsiteClientID]          INT             NOT NULL,
    [ClientWalletID]           INT             NOT NULL,
    [ClientBankAccountID]      INT             NOT NULL,
    [AmountUsd]                DECIMAL (18, 4) NOT NULL,
    [Status]                   TINYINT         CONSTRAINT [DF_ClientWalletWithdrawals_Status] DEFAULT ((1)) NOT NULL,
    [ReviewedByMemberID]       INT             NULL,
    [ReviewedAt]               DATETIME2 (0)   NULL,
    [RejectReason]             NVARCHAR (500)  NULL,
    [PaymentRefNumber]         NVARCHAR (100)  NULL,
    [PaidAt]                   DATETIME2 (0)   NULL,
    [RequestedAt]              DATETIME2 (0)   CONSTRAINT [DF_ClientWalletWithdrawals_RequestedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_ClientWalletWithdrawals] PRIMARY KEY CLUSTERED ([ClientWalletWithdrawalID] ASC),
    CONSTRAINT [FK_ClientWalletWithdrawals_ClientBankAccounts] FOREIGN KEY ([ClientBankAccountID]) REFERENCES [dbo].[ClientBankAccounts] ([ClientBankAccountID]),
    CONSTRAINT [FK_ClientWalletWithdrawals_ClientWallets] FOREIGN KEY ([ClientWalletID]) REFERENCES [dbo].[ClientWallets] ([ClientWalletID]),
    CONSTRAINT [FK_ClientWalletWithdrawals_Members] FOREIGN KEY ([ReviewedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_ClientWalletWithdrawals_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_ClientWalletWithdrawals_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID])
);

-- Client requests a cash-out; on request the amount is held via a ClientWalletTransactions
-- WithdrawalHold row (Type=3). Status: 1=Pending, 2=Approved, 3=Rejected, 4=Paid.
-- Admin approval (Status=2/4) triggers the bank transfer to ClientBankAccounts; rejection
-- (Status=3) reverses the hold with a WithdrawalReversed row (Type=4).
GO

CREATE NONCLUSTERED INDEX [IX_ClientWalletWithdrawals_ClientBankAccountID]
    ON [dbo].[ClientWalletWithdrawals] ([ClientBankAccountID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ClientWalletWithdrawals_ClientWalletID]
    ON [dbo].[ClientWalletWithdrawals] ([ClientWalletID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ClientWalletWithdrawals_ReviewedByMemberID]
    ON [dbo].[ClientWalletWithdrawals] ([ReviewedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ClientWalletWithdrawals_WebsiteClientID]
    ON [dbo].[ClientWalletWithdrawals] ([WebsiteClientID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ClientWalletWithdrawals_WebsiteID]
    ON [dbo].[ClientWalletWithdrawals] ([WebsiteID] ASC);
