CREATE TABLE [dbo].[FileRecordTags] (
    [FileRecordTagID] INT IDENTITY (1, 1) NOT NULL,
    [FileRecordID]    INT NOT NULL,
    [FileTagID]       INT NOT NULL,
    CONSTRAINT [PK_FileRecordTags] PRIMARY KEY CLUSTERED ([FileRecordTagID] ASC),
    CONSTRAINT [FK_FileRecordTags_FileRecords] FOREIGN KEY ([FileRecordID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_FileRecordTags_FileTags] FOREIGN KEY ([FileTagID]) REFERENCES [dbo].[FileTags] ([FileTagID])
);
