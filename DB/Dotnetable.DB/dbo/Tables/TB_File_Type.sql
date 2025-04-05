CREATE TABLE [dbo].[TB_File_Type] (
    [FileTypeID]    SMALLINT     IDENTITY (1, 1) NOT NULL,
    [FileTypeName]  VARCHAR (78) NOT NULL,
    [FileExtention] VARCHAR (5)  NOT NULL,
    [MIMEType]      VARCHAR (73) NOT NULL,
    CONSTRAINT [PK_TB_File_Type] PRIMARY KEY CLUSTERED ([FileTypeID] ASC)
);

