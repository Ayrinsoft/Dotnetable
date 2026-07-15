CREATE TABLE [dbo].[EmailAccounts] (
    [EmailAccountID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT            NOT NULL,
    [AccountType]     TINYINT        NOT NULL,
    [Name]            NVARCHAR (64)  NOT NULL,
    [EmailAddress]    VARCHAR (64)   NOT NULL,
    [Password]        NVARCHAR (256) NOT NULL,
    [MailServer]      VARCHAR (64)   NOT NULL,
    [SMTPPort]        INT            NOT NULL,
    [EnableSSL]       BIT            NOT NULL,
    [MailName]        NVARCHAR (64)  NOT NULL,
    [IsDefault]       BIT            NOT NULL,
    [Active]          BIT            NOT NULL,
    CONSTRAINT [PK_EmailAccounts] PRIMARY KEY CLUSTERED ([EmailAccountID] ASC),
    CONSTRAINT [FK_EmailAccounts_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_EmailAccounts_WebsiteID]
    ON [dbo].[EmailAccounts] ([WebsiteID] ASC);
