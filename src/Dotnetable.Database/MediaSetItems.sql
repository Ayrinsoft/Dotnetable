CREATE TABLE [dbo].[MediaSetItems] (
    [MediaSetItemID]       INT            IDENTITY (1, 1) NOT NULL,
    [MediaSetID]           INT            NOT NULL,
    [FileID]               INT            NULL,
    [VideoThumbnailFileID] INT            NULL,
    [ExternalVideoUrl]     NVARCHAR (500) NULL,
    [SortOrder]            INT NOT NULL,
    CONSTRAINT [PK_MediaSetItems] PRIMARY KEY CLUSTERED ([MediaSetItemID] ASC),
    CONSTRAINT [FK_MediaSetItems_FileRecords] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_MediaSetItems_FileRecord1] FOREIGN KEY ([VideoThumbnailFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_MediaSetItems_MediaSets] FOREIGN KEY ([MediaSetID]) REFERENCES [dbo].[MediaSets] ([MediaSetID])
);
GO

CREATE NONCLUSTERED INDEX [IX_MediaSetItems_FileID]
    ON [dbo].[MediaSetItems] ([FileID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_MediaSetItems_MediaSetID]
    ON [dbo].[MediaSetItems] ([MediaSetID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_MediaSetItems_VideoThumbnailFileID]
    ON [dbo].[MediaSetItems] ([VideoThumbnailFileID] ASC);
