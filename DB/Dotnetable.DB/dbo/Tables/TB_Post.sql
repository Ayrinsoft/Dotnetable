CREATE TABLE [dbo].[TB_Post] (
    [PostID]          INT              IDENTITY (1, 1) NOT NULL,
    [Title]           NVARCHAR (512)   NOT NULL,
    [Summary]         NVARCHAR (1024)  NOT NULL,
    [Body]            NVARCHAR (MAX)   NOT NULL,
    [PostCategoryID]  INT              NOT NULL,
    [Tags]            NVARCHAR (2000)  NULL,
    [MetaKeywords]    NVARCHAR (255)   NULL,
    [MetaDescription] NVARCHAR (255)   NULL,
    [MemberID]        INT              NOT NULL,
    [LogTime]         DATETIME         NOT NULL,
    [Active]          BIT              NOT NULL,
    [FileCode]        UNIQUEIDENTIFIER NULL,
    [PostCode]        VARCHAR (64)     NULL,
    [NormalBody]      BIT              NOT NULL,
    [VisitCount]      INT              NOT NULL,
    [LanguageCode]    CHAR (2)         NULL,
    CONSTRAINT [PK_TB_Post] PRIMARY KEY CLUSTERED ([PostID] ASC),
    CONSTRAINT [FK_TB_Post_TB_Member] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[TB_Member] ([MemberID]),
    CONSTRAINT [FK_TB_Post_TB_Post_Category] FOREIGN KEY ([PostCategoryID]) REFERENCES [dbo].[TB_Post_Category] ([PostCategoryID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Post_PostCategoryID]
    ON [dbo].[TB_Post]([PostCategoryID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Post_MemberID]
    ON [dbo].[TB_Post]([MemberID] ASC);

