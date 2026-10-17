CREATE TABLE [dbo].[QuizQuestions] (
    [QuizQuestionID] INT             IDENTITY (1, 1) NOT NULL,
    [QuizID]         INT             NOT NULL,
    [Prompt]         NVARCHAR (MAX)  NOT NULL,
    [Points]         DECIMAL (9, 2)  NOT NULL,
    [AllowMultiple]  BIT             NOT NULL,
    [AllowText]      BIT             NOT NULL,
    [SortOrder]      INT             NOT NULL,
    [IsActive]       BIT             NOT NULL,
    CONSTRAINT [PK_QuizQuestions] PRIMARY KEY CLUSTERED ([QuizQuestionID] ASC),
    CONSTRAINT [FK_QuizQuestions_Quizzes] FOREIGN KEY ([QuizID]) REFERENCES [dbo].[Quizzes] ([QuizID]) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_QuizQuestions_QuizID]
    ON [dbo].[QuizQuestions] ([QuizID] ASC);
