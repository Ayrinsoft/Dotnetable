CREATE TABLE [dbo].[WebsiteTelegramAccounts] (
    [WebsiteTelegramAccountID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]                INT            NOT NULL,
    [BotToken]                 NVARCHAR (MAX) NOT NULL,
    [ChatId]                   NVARCHAR (80)  NOT NULL,
    [Username]                 NVARCHAR (100) NULL,
    [IsActive]                 BIT            NOT NULL,
    [UpdatedAt]                DATETIME       NOT NULL,
    CONSTRAINT [PK_WebsiteTelegramAccounts] PRIMARY KEY CLUSTERED ([WebsiteTelegramAccountID] ASC),
    CONSTRAINT [FK_WebsiteTelegramAccounts_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_WebsiteTelegramAccounts_WebsiteID]
    ON [dbo].[WebsiteTelegramAccounts] ([WebsiteID] ASC);
GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Telegram bot a website publishes through. BotToken is encrypted.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'WebsiteTelegramAccounts';
GO
