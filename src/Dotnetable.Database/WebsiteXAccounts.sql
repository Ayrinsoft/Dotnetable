CREATE TABLE [dbo].[WebsiteXAccounts] (
    [WebsiteXAccountID]  INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]          INT            NOT NULL,
    [ApiKey]             NVARCHAR (MAX) NOT NULL,
    [ApiSecret]          NVARCHAR (MAX) NOT NULL,
    [AccessToken]        NVARCHAR (MAX) NOT NULL,
    [AccessTokenSecret]  NVARCHAR (MAX) NOT NULL,
    [Username]           NVARCHAR (100) NULL,
    [IsActive]           BIT            NOT NULL,
    [UpdatedAt]          DATETIME       NOT NULL,
    CONSTRAINT [PK_WebsiteXAccounts] PRIMARY KEY CLUSTERED ([WebsiteXAccountID] ASC),
    CONSTRAINT [FK_WebsiteXAccounts_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_WebsiteXAccounts_WebsiteID]
    ON [dbo].[WebsiteXAccounts] ([WebsiteID] ASC);
GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'X user credentials for posting. The four key columns are encrypted.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'WebsiteXAccounts';
GO
