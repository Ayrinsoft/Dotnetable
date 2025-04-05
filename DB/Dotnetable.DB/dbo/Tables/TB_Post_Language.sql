CREATE TABLE [dbo].[TB_Post_Language] (
    [PostLanguageID]  INT             IDENTITY (1, 1) NOT NULL,
    [LanguageCode]    CHAR (2)        NOT NULL,
    [PostID]          INT             NOT NULL,
    [Title]           NVARCHAR (512)  NOT NULL,
    [Summary]         NVARCHAR (1024) NOT NULL,
    [Body]            NVARCHAR (MAX)  NOT NULL,
    [Tags]            NVARCHAR (2000) NULL,
    [MetaKeywords]    NVARCHAR (255)  NULL,
    [MetaDescription] NVARCHAR (255)  NULL,
    CONSTRAINT [PK_TB_L_Post] PRIMARY KEY CLUSTERED ([PostLanguageID] ASC),
    CONSTRAINT [FK_TB_Post_Language_TB_Post] FOREIGN KEY ([PostID]) REFERENCES [dbo].[TB_Post] ([PostID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Post_Language_PostID]
    ON [dbo].[TB_Post_Language]([PostID] ASC);

