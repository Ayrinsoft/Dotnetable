CREATE TABLE [dbo].[PaymentRefunds] (
    [PaymentRefundID]   INT             IDENTITY (1, 1) NOT NULL,
    [PaymentID]         INT             NOT NULL,
    [Amount]            DECIMAL (18, 4) NOT NULL,
    [Reason]            NVARCHAR (500)  NULL,
    [Status]            TINYINT         CONSTRAINT [DF_PaymentRefunds_Status] DEFAULT ((1)) NOT NULL,
    [BankAccountID]     INT             NULL,
    [ClientWalletTransactionID] INT     NULL,
    [RefundedAt]        DATETIME        NULL,
    [CreatedByMemberID] INT             NULL,
    [CreatedAt]         DATETIME        CONSTRAINT [DF_PaymentRefunds_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_PaymentRefunds] PRIMARY KEY CLUSTERED ([PaymentRefundID] ASC),
    CONSTRAINT [FK_PaymentRefunds_BankAccounts] FOREIGN KEY ([BankAccountID]) REFERENCES [dbo].[BankAccounts] ([BankAccountID]),
    CONSTRAINT [FK_PaymentRefunds_ClientWalletTransactions] FOREIGN KEY ([ClientWalletTransactionID]) REFERENCES [dbo].[ClientWalletTransactions] ([ClientWalletTransactionID]),
    CONSTRAINT [FK_PaymentRefunds_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_PaymentRefunds_Payments] FOREIGN KEY ([PaymentID]) REFERENCES [dbo].[Payments] ([PaymentID])
);

-- Exactly one of BankAccountID / ClientWalletTransactionID should be set once RefundedAt is
-- populated: BankAccountID = refunded to the website's bank account (cash out),
-- ClientWalletTransactionID = credited to the customer's ClientWallets balance instead.

