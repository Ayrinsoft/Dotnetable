CREATE TABLE [dbo].[Quizzes] (
    [QuizID]             INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]          INT             NOT NULL,
    [QuizCategoryID]     INT             NOT NULL,
    [Title]              NVARCHAR (200)  NOT NULL,
    [Slug]               NVARCHAR (160)  NOT NULL,
    [Summary]            NVARCHAR (2000) NULL,
    [DurationMinutes]    INT             NOT NULL,
    [QuestionMode]       TINYINT         NOT NULL,
    [RandomCount]        INT             NULL,
    [IsPublished]        BIT             NOT NULL,
    [OfferPrivateReview] BIT             NOT NULL,
    [PrivateReviewFee]   DECIMAL (18, 4) NOT NULL,
    [CurrencyCode]       NVARCHAR (3)    NULL,
    [CreatedAt]          DATETIME        NOT NULL,
    [UpdatedAt]          DATETIME        NOT NULL,
    CONSTRAINT [PK_Quizzes] PRIMARY KEY CLUSTERED ([QuizID] ASC),
    CONSTRAINT [FK_Quizzes_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_Quizzes_QuizCategories] FOREIGN KEY ([QuizCategoryID]) REFERENCES [dbo].[QuizCategories] ([QuizCategoryID])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_Quizzes_WebsiteID_Slug]
    ON [dbo].[Quizzes] ([WebsiteID] ASC, [Slug] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Quizzes_QuizCategoryID]
    ON [dbo].[Quizzes] ([QuizCategoryID] ASC);
