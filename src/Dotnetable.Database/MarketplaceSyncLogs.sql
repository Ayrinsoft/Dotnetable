CREATE TABLE [dbo].[MarketplaceSyncLogs] (
    [MarketplaceSyncLogID] INT             IDENTITY (1, 1) NOT NULL,
    [MarketplaceChannelID] INT             NOT NULL,
    [StartedAt]            DATETIME2 (0)   NOT NULL,
    [FinishedAt]           DATETIME2 (0)   NULL,
    [Status]               TINYINT         NOT NULL,
    [TriggeredBy]          VARCHAR (20)    NOT NULL,
    [ItemCount]            INT             NOT NULL,
    [FailedCount]          INT             NOT NULL,
    [Message]              NVARCHAR (1000) NULL,
    CONSTRAINT [PK_MarketplaceSyncLogs] PRIMARY KEY CLUSTERED ([MarketplaceSyncLogID] ASC),
    CONSTRAINT [FK_MarketplaceSyncLogs_MarketplaceChannels] FOREIGN KEY ([MarketplaceChannelID]) REFERENCES [dbo].[MarketplaceChannels] ([MarketplaceChannelID]) ON DELETE CASCADE
);


-- One attempted push or feed build per row, so an admin can see what the last run did without
-- reading server logs. Status mirrors MarketplaceSyncStatus: 0 running, 1 success, 2 partial,
-- 3 failed. Trimmed to the most recent 100 rows per channel.
GO

CREATE NONCLUSTERED INDEX [IX_MarketplaceSyncLogs_Channel]
    ON [dbo].[MarketplaceSyncLogs] ([MarketplaceChannelID] ASC, [MarketplaceSyncLogID] ASC);
