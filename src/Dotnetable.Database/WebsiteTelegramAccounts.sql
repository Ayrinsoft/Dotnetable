CREATE TABLE [dbo].[WebsiteTelegramAccounts] (
    [WebsiteTelegramAccountID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]                INT            NOT NULL,
    [Title]                    NVARCHAR (80)  NOT NULL CONSTRAINT [DF_WebsiteTelegramAccounts_Title] DEFAULT (N''),
    [LanguageCode]             NVARCHAR (10)  NULL,
    [BotToken]                 NVARCHAR (MAX) NOT NULL,
    [ChatId]                   NVARCHAR (80)  NOT NULL,
    [Username]                 NVARCHAR (100) NULL,
    [IsActive]                 BIT            NOT NULL,
    [UpdatedAt]                DATETIME       NOT NULL,
    CONSTRAINT [PK_WebsiteTelegramAccounts] PRIMARY KEY CLUSTERED ([WebsiteTelegramAccountID] ASC),
    CONSTRAINT [FK_WebsiteTelegramAccounts_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteTelegramAccounts_WebsiteID]
    ON [dbo].[WebsiteTelegramAccounts] ([WebsiteID] ASC);
GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Telegram bots a website publishes through. A website may have several. BotToken is encrypted.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'WebsiteTelegramAccounts';
GO
