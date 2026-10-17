CREATE TABLE [dbo].[QuizAttemptQuestions] (
    [QuizAttemptID]  INT NOT NULL,
    [QuizQuestionID] INT NOT NULL,
    [SortOrder]      INT NOT NULL,
    CONSTRAINT [PK_QuizAttemptQuestions] PRIMARY KEY CLUSTERED ([QuizAttemptID] ASC, [QuizQuestionID] ASC),
    CONSTRAINT [FK_QuizAttemptQuestions_QuizAttempts] FOREIGN KEY ([QuizAttemptID]) REFERENCES [dbo].[QuizAttempts] ([QuizAttemptID]) ON DELETE CASCADE,
    CONSTRAINT [FK_QuizAttemptQuestions_QuizQuestions] FOREIGN KEY ([QuizQuestionID]) REFERENCES [dbo].[QuizQuestions] ([QuizQuestionID])
);
