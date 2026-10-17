CREATE TABLE [dbo].[QuizOptions] (
    [QuizOptionID]   INT             IDENTITY (1, 1) NOT NULL,
    [QuizQuestionID] INT             NOT NULL,
    [Text]           NVARCHAR (1000) NOT NULL,
    [IsCorrect]      BIT             NOT NULL,
    [SortOrder]      INT             NOT NULL,
    CONSTRAINT [PK_QuizOptions] PRIMARY KEY CLUSTERED ([QuizOptionID] ASC),
    CONSTRAINT [FK_QuizOptions_QuizQuestions] FOREIGN KEY ([QuizQuestionID]) REFERENCES [dbo].[QuizQuestions] ([QuizQuestionID]) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_QuizOptions_QuizQuestionID]
    ON [dbo].[QuizOptions] ([QuizQuestionID] ASC);
