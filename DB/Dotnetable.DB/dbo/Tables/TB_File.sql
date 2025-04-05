CREATE TABLE [dbo].[TB_File] (
    [FileID]         INT              IDENTITY (1, 1) NOT NULL,
    [FileCode]       UNIQUEIDENTIFIER NOT NULL,
    [FileName]       NVARCHAR (64)    NOT NULL,
    [FileTypeID]     SMALLINT         NOT NULL,
    [UploadTime]     DATETIME         NOT NULL,
    [Approved]       BIT              NOT NULL,
    [FilePath]       VARCHAR (60)     NOT NULL,
    [FileCategoryID] TINYINT          NOT NULL,
    [UploaderID]     INT              NOT NULL,
    CONSTRAINT [PK_TB_File] PRIMARY KEY CLUSTERED ([FileID] ASC),
    CONSTRAINT [FK_TB_File_TB_File_Category] FOREIGN KEY ([FileCategoryID]) REFERENCES [dbo].[TB_File_Category] ([FileCategoryID]),
    CONSTRAINT [FK_TB_File_TB_File_Type] FOREIGN KEY ([FileTypeID]) REFERENCES [dbo].[TB_File_Type] ([FileTypeID]),
    CONSTRAINT [FK_TB_File_TB_Member] FOREIGN KEY ([UploaderID]) REFERENCES [dbo].[TB_Member] ([MemberID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_File_UploaderID]
    ON [dbo].[TB_File]([UploaderID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TB_File_FileTypeID]
    ON [dbo].[TB_File]([FileTypeID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TB_File_FileCategoryID]
    ON [dbo].[TB_File]([FileCategoryID] ASC);

