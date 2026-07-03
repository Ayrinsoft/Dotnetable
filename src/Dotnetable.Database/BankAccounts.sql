CREATE TABLE [dbo].[BankAccounts] (
    [BankAccountID]       INT           IDENTITY (1, 1) NOT NULL,
    [BankID]              INT           NOT NULL,
    [WebsiteID]           INT           NOT NULL,
    [Title]               NVARCHAR (70) NOT NULL,
    [OwnerName]           NVARCHAR (90) NULL,
    [AccountNumber]       VARCHAR (30)  NULL,
    [IBAN]                VARCHAR (34)  NULL,
    [CardNumber]          VARCHAR (20)  NULL,
    [IsForOfflinePayment] BIT           CONSTRAINT [DF_BankAccounts_IsForOfflinePayment] DEFAULT ((1)) NOT NULL,
    [IsActive]            BIT           CONSTRAINT [DF_BankAccounts_IsActive] DEFAULT ((1)) NOT NULL,
    [CreatedByMemberId]   INT           NOT NULL,
    CONSTRAINT [PK_BankAccounts] PRIMARY KEY CLUSTERED ([BankAccountID] ASC),
    CONSTRAINT [FK_BankAccounts_Bank] FOREIGN KEY ([BankID]) REFERENCES [dbo].[Bank] ([BankID]),
    CONSTRAINT [FK_BankAccounts_Member] FOREIGN KEY ([CreatedByMemberId]) REFERENCES [dbo].[Member] ([MemberID]),
    CONSTRAINT [FK_BankAccounts_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

