CREATE TABLE [dbo].[FileRecordTag] (
    [FileRecordTagID] INT IDENTITY (1, 1) NOT NULL,
    [FileRecordID]    INT NOT NULL,
    [FileTagID]       INT NOT NULL,
    CONSTRAINT [PK_FileRecordTag] PRIMARY KEY CLUSTERED ([FileRecordTagID] ASC),
    CONSTRAINT [FK_FileRecordTag_FileRecord] FOREIGN KEY ([FileRecordID]) REFERENCES [dbo].[FileRecord] ([FileRecordID]),
    CONSTRAINT [FK_FileRecordTag_FileTag] FOREIGN KEY ([FileTagID]) REFERENCES [dbo].[FileTag] ([FileTagID])
);
