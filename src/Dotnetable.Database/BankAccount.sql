CREATE TABLE [dbo].[BankAccount] (
    [BankAccountID]       INT            IDENTITY (1, 1) NOT NULL,
    [BankID]              INT            NOT NULL,
    [WebsiteID]           INT            NOT NULL,
    [Title]               NVARCHAR (70)  NOT NULL,
    [OwnerName]           NVARCHAR (90)  NULL,
    [AccountNumber]       VARCHAR (30)   NULL,
    [IBAN]                VARCHAR (34)   NULL,
    [CardNumber]          VARCHAR (20)   NULL,
    [IsForOfflinePayment] BIT            CONSTRAINT [DF_BankAccount_IsForOfflinePayment] DEFAULT ((1)) NOT NULL,
    [IsActive]            BIT            CONSTRAINT [DF_BankAccount_IsActive] DEFAULT ((1)) NOT NULL,
    [CreatedByMemberId]   INT            NOT NULL,
    [JSONSettings]        VARCHAR (2000) NOT NULL,
    CONSTRAINT [PK_BankAccount] PRIMARY KEY CLUSTERED ([BankAccountID] ASC),
    CONSTRAINT [FK_BankAccount_Bank] FOREIGN KEY ([BankID]) REFERENCES [dbo].[Bank] ([BankID]),
    CONSTRAINT [FK_BankAccount_Member] FOREIGN KEY ([CreatedByMemberId]) REFERENCES [dbo].[Member] ([MemberID]),
    CONSTRAINT [FK_BankAccount_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

