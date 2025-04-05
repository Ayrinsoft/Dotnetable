CREATE TABLE [dbo].[TBM_Member_File] (
    [MemberFileID] INT IDENTITY (1, 1) NOT NULL,
    [MemberID]     INT NOT NULL,
    [FileID]       INT NOT NULL,
    CONSTRAINT [PK_TBM_Member_File] PRIMARY KEY CLUSTERED ([MemberFileID] ASC),
    CONSTRAINT [FK_TBM_Member_File_TB_File] FOREIGN KEY ([FileID]) REFERENCES [dbo].[TB_File] ([FileID]),
    CONSTRAINT [FK_TBM_Member_File_TB_Member] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[TB_Member] ([MemberID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TBM_Member_File_MemberID]
    ON [dbo].[TBM_Member_File]([MemberID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TBM_Member_File_FileID]
    ON [dbo].[TBM_Member_File]([FileID] ASC);

