CREATE TABLE [dbo].[Payments] (
    [PaymentID]          INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]          INT             NOT NULL,
    [OrderID]            INT             NULL,
    [WebsiteClientID]    INT             NOT NULL,
    [Method]             TINYINT         NOT NULL,
    [PaymentGatewayID]   INT             NULL,
    [BankAccountID]      INT             NULL,
    [ClientWalletTransactionID] INT      NULL,
    [Amount]             DECIMAL (18, 4) NOT NULL,
    [CurrencyCode]       CHAR (3)        NOT NULL,
    [ExchangeRateToUsd]  DECIMAL (18, 6) NOT NULL,
    [AmountUsd]          DECIMAL (18, 4) NOT NULL,
    [Status]             TINYINT NOT NULL,
    [GatewayRefNumber]   NVARCHAR (100)  NULL,
    [TrackingCode]       NVARCHAR (100)  NULL,
    [ReceiptFileID]      INT             NULL,
    [PaidAt]             DATETIME2 (0)   NULL,
    [VerifiedByMemberID] INT             NULL,
    [CreatedAt]          DATETIME2 (0) NOT NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY CLUSTERED ([PaymentID] ASC),
    CONSTRAINT [FK_Payments_BankAccounts] FOREIGN KEY ([BankAccountID]) REFERENCES [dbo].[BankAccounts] ([BankAccountID]),
    CONSTRAINT [FK_Payments_ClientWalletTransactions] FOREIGN KEY ([ClientWalletTransactionID]) REFERENCES [dbo].[ClientWalletTransactions] ([ClientWalletTransactionID]),
    CONSTRAINT [FK_Payments_Currencies] FOREIGN KEY ([CurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode]),
    CONSTRAINT [FK_Payments_FileRecords] FOREIGN KEY ([ReceiptFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_Payments_Members] FOREIGN KEY ([VerifiedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_Payments_Orders] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_Payments_PaymentGateways] FOREIGN KEY ([PaymentGatewayID]) REFERENCES [dbo].[PaymentGateways] ([PaymentGatewayID]),
    CONSTRAINT [FK_Payments_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_Payments_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID])
);

-- ClientWalletTransactionID is set when Method = Wallet: it points at the PurchaseUse ledger
-- row (ClientWalletTransactions.Type=2) that debited the customer's wallet for this payment.
GO

CREATE NONCLUSTERED INDEX [IX_Payments_BankAccountID]
    ON [dbo].[Payments] ([BankAccountID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Payments_ClientWalletTransactionID]
    ON [dbo].[Payments] ([ClientWalletTransactionID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Payments_CurrencyCode]
    ON [dbo].[Payments] ([CurrencyCode] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Payments_OrderID]
    ON [dbo].[Payments] ([OrderID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Payments_PaymentGatewayID]
    ON [dbo].[Payments] ([PaymentGatewayID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Payments_ReceiptFileID]
    ON [dbo].[Payments] ([ReceiptFileID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Payments_VerifiedByMemberID]
    ON [dbo].[Payments] ([VerifiedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Payments_WebsiteClientID]
    ON [dbo].[Payments] ([WebsiteClientID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Payments_WebsiteID]
    ON [dbo].[Payments] ([WebsiteID] ASC);
