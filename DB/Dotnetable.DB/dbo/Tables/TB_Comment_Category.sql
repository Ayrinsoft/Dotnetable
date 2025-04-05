CREATE TABLE [dbo].[TB_Comment_Category] (
    [CommentCategoryID] TINYINT     NOT NULL,
    [Title]             VARCHAR (8) NOT NULL,
    CONSTRAINT [PK_TB_Comment_Category] PRIMARY KEY CLUSTERED ([CommentCategoryID] ASC)
);

