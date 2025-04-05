CREATE TABLE [dbo].[TB_Post_Category_Language] (
    [PostCategoryLanguageID] INT             IDENTITY (1, 1) NOT NULL,
    [PostCategoryID]         INT             NOT NULL,
    [Title]                  NVARCHAR (128)  NOT NULL,
    [LanguageCode]           CHAR (2)        NOT NULL,
    [Tags]                   NVARCHAR (2000) NULL,
    [MetaKeywords]           NVARCHAR (256)  NULL,
    [MetaDescription]        NVARCHAR (256)  NULL,
    [Description]            NVARCHAR (512)  NOT NULL,
    CONSTRAINT [PK_TB_L_Post_Category] PRIMARY KEY CLUSTERED ([PostCategoryLanguageID] ASC),
    CONSTRAINT [FK_TB_L_Post_Category_TB_Post_Category] FOREIGN KEY ([PostCategoryID]) REFERENCES [dbo].[TB_Post_Category] ([PostCategoryID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Post_Category_Language_PostCategoryID]
    ON [dbo].[TB_Post_Category_Language]([PostCategoryID] ASC);

