CREATE TABLE [dbo].[QuizAttemptAnswers] (
    [QuizAttemptAnswerID] INT             IDENTITY (1, 1) NOT NULL,
    [QuizAttemptID]       INT             NOT NULL,
    [QuizQuestionID]      INT             NOT NULL,
    [TextAnswer]          NVARCHAR (MAX)  NULL,
    [PointsAwarded]       DECIMAL (9, 2)  NOT NULL,
    CONSTRAINT [PK_QuizAttemptAnswers] PRIMARY KEY CLUSTERED ([QuizAttemptAnswerID] ASC),
    CONSTRAINT [FK_QuizAttemptAnswers_QuizAttempts] FOREIGN KEY ([QuizAttemptID]) REFERENCES [dbo].[QuizAttempts] ([QuizAttemptID]) ON DELETE CASCADE,
    CONSTRAINT [FK_QuizAttemptAnswers_QuizQuestions] FOREIGN KEY ([QuizQuestionID]) REFERENCES [dbo].[QuizQuestions] ([QuizQuestionID])
);
GO

CREATE NONCLUSTERED INDEX [IX_QuizAttemptAnswers_QuizAttemptID]
    ON [dbo].[QuizAttemptAnswers] ([QuizAttemptID] ASC);
