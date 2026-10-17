CREATE TABLE [dbo].[QuizScoreBandTranslations] (
    [QuizScoreBandTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [QuizScoreBandID]            INT            NOT NULL,
    [LanguageCode]               CHAR (2)       NOT NULL,
    [Title]                      NVARCHAR (200) NULL,
    [Body]                       NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_QuizScoreBandTranslations] PRIMARY KEY CLUSTERED ([QuizScoreBandTranslationID] ASC),
    CONSTRAINT [FK_QuizScoreBandTranslations_QuizScoreBands] FOREIGN KEY ([QuizScoreBandID]) REFERENCES [dbo].[QuizScoreBands] ([QuizScoreBandID]) ON DELETE CASCADE
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_QuizScoreBandTranslations_Band_Language]
    ON [dbo].[QuizScoreBandTranslations] ([QuizScoreBandID] ASC, [LanguageCode] ASC);
