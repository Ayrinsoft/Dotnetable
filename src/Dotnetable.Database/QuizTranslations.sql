CREATE TABLE [dbo].[QuizTranslations] (
    [QuizTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [QuizID]            INT            NOT NULL,
    [LanguageCode]      CHAR (2)       NOT NULL,
    [Title]             NVARCHAR (200) NULL,
    [Summary]           NVARCHAR (2000) NULL,
    CONSTRAINT [PK_QuizTranslations] PRIMARY KEY CLUSTERED ([QuizTranslationID] ASC),
    CONSTRAINT [FK_QuizTranslations_Quizzes] FOREIGN KEY ([QuizID]) REFERENCES [dbo].[Quizzes] ([QuizID]) ON DELETE CASCADE
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_QuizTranslations_Quiz_Language]
    ON [dbo].[QuizTranslations] ([QuizID] ASC, [LanguageCode] ASC);
