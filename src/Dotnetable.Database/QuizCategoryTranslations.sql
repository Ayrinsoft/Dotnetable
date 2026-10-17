CREATE TABLE [dbo].[QuizCategoryTranslations] (
    [QuizCategoryTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [QuizCategoryID]            INT            NOT NULL,
    [LanguageCode]              CHAR (2)       NOT NULL,
    [Title]                     NVARCHAR (200) NULL,
    [Description]               NVARCHAR (2000) NULL,
    CONSTRAINT [PK_QuizCategoryTranslations] PRIMARY KEY CLUSTERED ([QuizCategoryTranslationID] ASC),
    CONSTRAINT [FK_QuizCategoryTranslations_QuizCategories] FOREIGN KEY ([QuizCategoryID]) REFERENCES [dbo].[QuizCategories] ([QuizCategoryID]) ON DELETE CASCADE
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_QuizCategoryTranslations_Category_Language]
    ON [dbo].[QuizCategoryTranslations] ([QuizCategoryID] ASC, [LanguageCode] ASC);
