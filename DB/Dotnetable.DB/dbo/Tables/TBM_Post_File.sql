CREATE TABLE [dbo].[TBM_Post_File] (
    [PostFileID]  INT IDENTITY (1, 1) NOT NULL,
    [PostID]      INT NOT NULL,
    [FileID]      INT NOT NULL,
    [ShowGallery] BIT NOT NULL,
    CONSTRAINT [PK_TBM_Post_File] PRIMARY KEY CLUSTERED ([PostFileID] ASC),
    CONSTRAINT [FK_TBM_Post_File_TB_File] FOREIGN KEY ([FileID]) REFERENCES [dbo].[TB_File] ([FileID]),
    CONSTRAINT [FK_TBM_Post_File_TB_Post] FOREIGN KEY ([PostID]) REFERENCES [dbo].[TB_Post] ([PostID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TBM_Post_File_PostID]
    ON [dbo].[TBM_Post_File]([PostID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TBM_Post_File_FileID]
    ON [dbo].[TBM_Post_File]([FileID] ASC);

