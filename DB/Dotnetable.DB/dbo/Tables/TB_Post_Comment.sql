CREATE TABLE [dbo].[TB_Post_Comment] (
    [PostCommentID]      INT            IDENTITY (1, 1) NOT NULL,
    [CommentTypeID]      TINYINT        NOT NULL,
    [LogTime]            DATETIME       NOT NULL,
    [Active]             BIT            NULL,
    [CommentBody]        NVARCHAR (512) NOT NULL,
    [LanguageCode]       CHAR (2)       NOT NULL,
    [PostID]             INT            NOT NULL,
    [MemberID]           INT            NOT NULL,
    [IPAddress]          VARCHAR (15)   NOT NULL,
    [ReplyPostCommentID] INT            NULL,
    CONSTRAINT [PK_TB_Post_Comment] PRIMARY KEY CLUSTERED ([PostCommentID] ASC),
    CONSTRAINT [FK_TB_Post_Comment_TB_Comment_Type] FOREIGN KEY ([CommentTypeID]) REFERENCES [dbo].[TB_Comment_Type] ([CommentTypeID]),
    CONSTRAINT [FK_TB_Post_Comment_TB_Member] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[TB_Member] ([MemberID]),
    CONSTRAINT [FK_TB_Post_Comment_TB_Post] FOREIGN KEY ([PostID]) REFERENCES [dbo].[TB_Post] ([PostID]),
    CONSTRAINT [FK_TB_Post_Comment_TB_Post_Comment] FOREIGN KEY ([ReplyPostCommentID]) REFERENCES [dbo].[TB_Post_Comment] ([PostCommentID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Post_Comment_ReplyPostCommentID]
    ON [dbo].[TB_Post_Comment]([ReplyPostCommentID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Post_Comment_PostID]
    ON [dbo].[TB_Post_Comment]([PostID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Post_Comment_MemberID]
    ON [dbo].[TB_Post_Comment]([MemberID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Post_Comment_CommentTypeID]
    ON [dbo].[TB_Post_Comment]([CommentTypeID] ASC);

