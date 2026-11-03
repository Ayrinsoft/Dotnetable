CREATE TABLE [dbo].[PostImages] (
    [PostImageID]  INT IDENTITY (1, 1) NOT NULL,
    [PostID]       INT NOT NULL,
    [FileRecordID] INT NOT NULL,
    [SortOrder]    INT NOT NULL,
    CONSTRAINT [PK_PostImages] PRIMARY KEY CLUSTERED ([PostImageID] ASC),
    CONSTRAINT [FK_PostImages_FileRecords] FOREIGN KEY ([FileRecordID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_PostImages_Posts] FOREIGN KEY ([PostID]) REFERENCES [dbo].[Posts] ([PostID])
);
GO

CREATE NONCLUSTERED INDEX [IX_PostImages_FileRecordID]
    ON [dbo].[PostImages] ([FileRecordID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_PostImages_PostID]
    ON [dbo].[PostImages] ([PostID] ASC);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_PostImages_Post_File]
    ON [dbo].[PostImages] ([PostID] ASC, [FileRecordID] ASC);
GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Gallery photo. The lowest SortOrder is the post cover and matches Posts.FeaturedImageFileID.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PostImages';
GO
