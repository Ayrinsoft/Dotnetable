CREATE TABLE [dbo].[QuizScoreBands] (
    [QuizScoreBandID] INT             IDENTITY (1, 1) NOT NULL,
    [QuizID]          INT             NOT NULL,
    [MinPercent]      DECIMAL (5, 2)  NOT NULL,
    [MaxPercent]      DECIMAL (5, 2)  NOT NULL,
    [Title]           NVARCHAR (200)  NOT NULL,
    [Body]            NVARCHAR (MAX)  NOT NULL,
    [SortOrder]       INT             NOT NULL,
    CONSTRAINT [PK_QuizScoreBands] PRIMARY KEY CLUSTERED ([QuizScoreBandID] ASC),
    CONSTRAINT [FK_QuizScoreBands_Quizzes] FOREIGN KEY ([QuizID]) REFERENCES [dbo].[Quizzes] ([QuizID]) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_QuizScoreBands_QuizID]
    ON [dbo].[QuizScoreBands] ([QuizID] ASC);
