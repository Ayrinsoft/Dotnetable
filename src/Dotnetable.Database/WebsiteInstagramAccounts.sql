CREATE TABLE [dbo].[WebsiteInstagramAccounts] (
    [WebsiteInstagramAccountID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]                 INT            NOT NULL,
    [Title]                     NVARCHAR (80)  NOT NULL CONSTRAINT [DF_WebsiteInstagramAccounts_Title] DEFAULT (N''),
    [LanguageCode]              NVARCHAR (10)  NULL,
    [IgUserId]                  NVARCHAR (40)  NOT NULL,
    [AccessToken]               NVARCHAR (MAX) NOT NULL,
    [PageId]                    NVARCHAR (40)  NULL,
    [Username]                  NVARCHAR (100) NULL,
    [IsActive]                  BIT            NOT NULL,
    [UpdatedAt]                 DATETIME       NOT NULL,
    CONSTRAINT [PK_WebsiteInstagramAccounts] PRIMARY KEY CLUSTERED ([WebsiteInstagramAccountID] ASC),
    CONSTRAINT [FK_WebsiteInstagramAccounts_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteInstagramAccounts_WebsiteID]
    ON [dbo].[WebsiteInstagramAccounts] ([WebsiteID] ASC);
GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Instagram professional accounts a website publishes to. A website may have several. AccessToken is a long-lived Page token.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'WebsiteInstagramAccounts';
GO
