CREATE TABLE [dbo].[DigitalAccessLogs] (
    [DigitalAccessLogID]  BIGINT         IDENTITY (1, 1) NOT NULL,
    [OrderDigitalAssetID] INT            NOT NULL,
    [WebsiteClientID]     INT            NOT NULL,
    [WebsiteID]           INT            NOT NULL,
    [AccessType]          TINYINT        NOT NULL,
    [IpAddress]           NVARCHAR (64)  NULL,
    [UserAgent]           NVARCHAR (500) NULL,
    [AccessedAt]          DATETIME       NOT NULL,
    CONSTRAINT [PK_DigitalAccessLogs] PRIMARY KEY CLUSTERED ([DigitalAccessLogID] ASC),
    CONSTRAINT [FK_DigitalAccessLogs_OrderDigitalAssets] FOREIGN KEY ([OrderDigitalAssetID]) REFERENCES [dbo].[OrderDigitalAssets] ([OrderDigitalAssetID]),
    CONSTRAINT [FK_DigitalAccessLogs_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID]),
    CONSTRAINT [FK_DigitalAccessLogs_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_DigitalAccessLogs_OrderDigitalAssetID]
    ON [dbo].[DigitalAccessLogs] ([OrderDigitalAssetID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_DigitalAccessLogs_WebsiteClientID]
    ON [dbo].[DigitalAccessLogs] ([WebsiteClientID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_DigitalAccessLogs_WebsiteID]
    ON [dbo].[DigitalAccessLogs] ([WebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_DigitalAccessLogs_AccessedAt]
    ON [dbo].[DigitalAccessLogs] ([AccessedAt] ASC);
