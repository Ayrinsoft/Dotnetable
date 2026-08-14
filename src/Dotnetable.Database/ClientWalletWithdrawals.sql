CREATE TABLE [dbo].[ClientWalletWithdrawals] (
    [ClientWalletWithdrawalID] INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]                INT             NOT NULL,
    [WebsiteClientID]          INT             NOT NULL,
    [ClientWalletID]           INT             NOT NULL,
    [ClientBankAccountID]      INT             NOT NULL,
    [AmountUsd]                DECIMAL (18, 4) NOT NULL,
    [Status]                   TINYINT         NOT NULL,
    [ReviewedByMemberID]       INT             NULL,
    [ReviewedAt]               DATETIME2 (0)   NULL,
    [RejectReason]             NVARCHAR (500)  NULL,
    [PaymentRefNumber]         NVARCHAR (100)  NULL,
    [Note]                     NVARCHAR (500)  NULL,
    [CreatedByMemberID]        INT             NULL,
    [PaidAt]                   DATETIME2 (0)   NULL,
    [RequestedAt]              DATETIME2 (0)   NOT NULL,
    [Amount]                   DECIMAL (18, 4) DEFAULT ((0.0)) NOT NULL,
    [CurrencyCode]             CHAR (3)        NOT NULL,
    CONSTRAINT [PK_ClientWalletWithdrawals] PRIMARY KEY CLUSTERED ([ClientWalletWithdrawalID] ASC),
    CONSTRAINT [FK_ClientWalletWithdrawals_ClientBankAccounts] FOREIGN KEY ([ClientBankAccountID]) REFERENCES [dbo].[ClientBankAccounts] ([ClientBankAccountID]),
    CONSTRAINT [FK_ClientWalletWithdrawals_ClientWallets] FOREIGN KEY ([ClientWalletID]) REFERENCES [dbo].[ClientWallets] ([ClientWalletID]),
    CONSTRAINT [FK_ClientWalletWithdrawals_Members] FOREIGN KEY ([ReviewedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_ClientWalletWithdrawals_Members_CreatedBy] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_ClientWalletWithdrawals_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID]),
    CONSTRAINT [FK_ClientWalletWithdrawals_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);



-- Cash-out from a specific currency wallet. Amount is in CurrencyCode.
-- Status: 1=Pending, 2=Approved, 3=Rejected, 4=Paid.
-- CreatedByMemberID is set when an admin records the payout (customer-requested rows stay NULL).
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

CREATE NONCLUSTERED INDEX [IX_ClientWalletWithdrawals_CreatedByMemberID]
    ON [dbo].[ClientWalletWithdrawals] ([CreatedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ClientWalletWithdrawals_WebsiteClientID]
    ON [dbo].[ClientWalletWithdrawals] ([WebsiteClientID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ClientWalletWithdrawals_WebsiteID]
    ON [dbo].[ClientWalletWithdrawals] ([WebsiteID] ASC);
GO

