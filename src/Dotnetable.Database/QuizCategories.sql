CREATE TABLE [dbo].[QuizCategories] (
    [QuizCategoryID]   INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT            NOT NULL,
    [ParentCategoryID] INT            NULL,
    [Title]            NVARCHAR (200) NOT NULL,
    [Slug]             NVARCHAR (160) NOT NULL,
    [Description]      NVARCHAR (2000) NULL,
    [SortOrder]        INT            NOT NULL,
    [IsActive]         BIT            NOT NULL,
    [ShowInPosts]      BIT            NOT NULL,
    [CreatedAt]        DATETIME       NOT NULL,
    CONSTRAINT [PK_QuizCategories] PRIMARY KEY CLUSTERED ([QuizCategoryID] ASC),
    CONSTRAINT [FK_QuizCategories_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_QuizCategories_QuizCategories] FOREIGN KEY ([ParentCategoryID]) REFERENCES [dbo].[QuizCategories] ([QuizCategoryID])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_QuizCategories_WebsiteID_Slug]
    ON [dbo].[QuizCategories] ([WebsiteID] ASC, [Slug] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_QuizCategories_ParentCategoryID]
    ON [dbo].[QuizCategories] ([ParentCategoryID] ASC);
