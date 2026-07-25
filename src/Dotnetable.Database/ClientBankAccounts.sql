CREATE TABLE [dbo].[ClientBankAccounts] (
    [ClientBankAccountID] INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]           INT           NOT NULL,
    [WebsiteClientID]     INT           NOT NULL,
    [BankID]              INT           NULL,
    [OwnerName]           NVARCHAR (90) NOT NULL,
    [AccountNumber]       VARCHAR (30)  NULL,
    [IBAN]                VARCHAR (34)  NULL,
    [CardNumber]          VARCHAR (20)  NULL,
    [IsDefault]           BIT           CONSTRAINT [DF_ClientBankAccounts_IsDefault] DEFAULT ((0)) NOT NULL,
    [IsActive]            BIT NOT NULL,
    [CreatedAt]           DATETIME2 (0) CONSTRAINT [DF_ClientBankAccounts_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_ClientBankAccounts] PRIMARY KEY CLUSTERED ([ClientBankAccountID] ASC),
    CONSTRAINT [FK_ClientBankAccounts_Banks] FOREIGN KEY ([BankID]) REFERENCES [dbo].[Banks] ([BankID]),
    CONSTRAINT [FK_ClientBankAccounts_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_ClientBankAccounts_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID])
);

-- The customer's own bank account, distinct from [dbo].[BankAccounts] (the website's accounts
-- used to receive offline payments). Withdrawal payouts from ClientWallets are sent here.
GO

CREATE NONCLUSTERED INDEX [IX_ClientBankAccounts_BankID]
    ON [dbo].[ClientBankAccounts] ([BankID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ClientBankAccounts_WebsiteClientID]
    ON [dbo].[ClientBankAccounts] ([WebsiteClientID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ClientBankAccounts_WebsiteID]
    ON [dbo].[ClientBankAccounts] ([WebsiteID] ASC);
