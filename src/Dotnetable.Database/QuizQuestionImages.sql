CREATE TABLE [dbo].[QuizQuestionImages] (
    [QuizQuestionImageID] INT IDENTITY (1, 1) NOT NULL,
    [QuizQuestionID]      INT NOT NULL,
    [FileRecordID]        INT NOT NULL,
    [SortOrder]           INT NOT NULL,
    CONSTRAINT [PK_QuizQuestionImages] PRIMARY KEY CLUSTERED ([QuizQuestionImageID] ASC),
    CONSTRAINT [FK_QuizQuestionImages_QuizQuestions] FOREIGN KEY ([QuizQuestionID]) REFERENCES [dbo].[QuizQuestions] ([QuizQuestionID]) ON DELETE CASCADE,
    CONSTRAINT [FK_QuizQuestionImages_FileRecords] FOREIGN KEY ([FileRecordID]) REFERENCES [dbo].[FileRecords] ([FileRecordID])
);
GO

CREATE NONCLUSTERED INDEX [IX_QuizQuestionImages_QuizQuestionID]
    ON [dbo].[QuizQuestionImages] ([QuizQuestionID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_QuizQuestionImages_FileRecordID]
    ON [dbo].[QuizQuestionImages] ([FileRecordID] ASC);
