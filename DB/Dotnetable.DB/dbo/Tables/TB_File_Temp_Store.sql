CREATE TABLE [dbo].[TB_File_Temp_Store] (
    [FileTempID] INT      IDENTITY (1, 1) NOT NULL,
    [FileID]     INT      NOT NULL,
    [ExpireTime] DATETIME NOT NULL,
    CONSTRAINT [PK_TB_T_File] PRIMARY KEY CLUSTERED ([FileTempID] ASC),
    CONSTRAINT [FK_TB_T_File_TB_File] FOREIGN KEY ([FileID]) REFERENCES [dbo].[TB_File] ([FileID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_File_Temp_Store_FileID]
    ON [dbo].[TB_File_Temp_Store]([FileID] ASC);

