CREATE TABLE [dbo].[BankAccounts] (
    [BankAccountID]       INT           IDENTITY (1, 1) NOT NULL,
    [BankID]              INT           NOT NULL,
    [WebsiteID]           INT           NOT NULL,
    [Title]               NVARCHAR (70) NOT NULL,
    [OwnerName]           NVARCHAR (90) NULL,
    [AccountNumber]       VARCHAR (30)  NULL,
    [IBAN]                VARCHAR (34)  NULL,
    [CardNumber]          VARCHAR (20)  NULL,
    [IsForOfflinePayment] BIT NOT NULL,
    [IsActive]            BIT NOT NULL,
    [CreatedByMemberID]   INT           NOT NULL,
    CONSTRAINT [PK_BankAccounts] PRIMARY KEY CLUSTERED ([BankAccountID] ASC),
    CONSTRAINT [FK_BankAccounts_Banks] FOREIGN KEY ([BankID]) REFERENCES [dbo].[Banks] ([BankID]),
    CONSTRAINT [FK_BankAccounts_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_BankAccounts_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_BankAccounts_BankID]
    ON [dbo].[BankAccounts] ([BankID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_BankAccounts_CreatedByMemberId]
    ON [dbo].[BankAccounts] ([CreatedByMemberId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_BankAccounts_WebsiteID]
    ON [dbo].[BankAccounts] ([WebsiteID] ASC);
