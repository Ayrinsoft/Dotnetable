CREATE TABLE [dbo].[TB_Comment_Type] (
    [CommentTypeID] TINYINT      NOT NULL,
    [Title]         VARCHAR (16) NOT NULL,
    CONSTRAINT [PK_TB_Comment_Type] PRIMARY KEY CLUSTERED ([CommentTypeID] ASC)
);

