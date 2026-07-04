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
    [Status]             TINYINT         CONSTRAINT [DF_Payments_Status_1] DEFAULT ((1)) NOT NULL,
    [GatewayRefNumber]   NVARCHAR (100)  NULL,
    [TrackingCode]       NVARCHAR (100)  NULL,
    [ReceiptFileID]      INT             NULL,
    [PaidAt]             DATETIME2 (0)   NULL,
    [VerifiedByMemberID] INT             NULL,
    [CreatedAt]          DATETIME2 (0)   CONSTRAINT [DF_Payments_CreatedAt_1] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY CLUSTERED ([PaymentID] ASC),
    CONSTRAINT [FK_Payments_BankAccounts] FOREIGN KEY ([BankAccountID]) REFERENCES [dbo].[BankAccounts] ([BankAccountID]),
    CONSTRAINT [FK_Payments_ClientWalletTransactions] FOREIGN KEY ([ClientWalletTransactionID]) REFERENCES [dbo].[ClientWalletTransactions] ([ClientWalletTransactionID]),
    CONSTRAINT [FK_Payments_FileRecords] FOREIGN KEY ([ReceiptFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_Payments_Members] FOREIGN KEY ([VerifiedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_Payments_Orders] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_Payments_PaymentGateways] FOREIGN KEY ([PaymentGatewayID]) REFERENCES [dbo].[PaymentGateways] ([PaymentGatewayID]),
    CONSTRAINT [FK_Payments_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_Payments_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID])
);

-- ClientWalletTransactionID is set when Method = Wallet: it points at the PurchaseUse ledger
-- row (ClientWalletTransactions.Type=2) that debited the customer's wallet for this payment.

