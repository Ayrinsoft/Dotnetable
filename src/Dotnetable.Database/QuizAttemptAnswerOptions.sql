CREATE TABLE [dbo].[QuizAttemptAnswerOptions] (
    [QuizAttemptAnswerID] INT NOT NULL,
    [QuizOptionID]        INT NOT NULL,
    CONSTRAINT [PK_QuizAttemptAnswerOptions] PRIMARY KEY CLUSTERED ([QuizAttemptAnswerID] ASC, [QuizOptionID] ASC),
    CONSTRAINT [FK_QuizAttemptAnswerOptions_QuizAttemptAnswers] FOREIGN KEY ([QuizAttemptAnswerID]) REFERENCES [dbo].[QuizAttemptAnswers] ([QuizAttemptAnswerID]) ON DELETE CASCADE,
    CONSTRAINT [FK_QuizAttemptAnswerOptions_QuizOptions] FOREIGN KEY ([QuizOptionID]) REFERENCES [dbo].[QuizOptions] ([QuizOptionID])
);
