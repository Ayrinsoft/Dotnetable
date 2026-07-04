CREATE TABLE [dbo].[ProductAnswers] (
    [ProductAnswerID]   INT             IDENTITY (1, 1) NOT NULL,
    [ProductQuestionID] INT             NOT NULL,
    [WebsiteClientID]   INT             NULL,
    [VendorID]          INT             NULL,
    [Body]              NVARCHAR (4000) NOT NULL,
    [Status]            TINYINT         CONSTRAINT [DF_ProductAnswers_Status] DEFAULT ((1)) NOT NULL,
    [LikeCount]         INT             CONSTRAINT [DF_ProductAnswers_LikeCount] DEFAULT ((0)) NOT NULL,
    [CreatedAt]         DATETIME        CONSTRAINT [DF_ProductAnswers_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    [Approved]          BIT             NOT NULL,
    CONSTRAINT [PK_ProductAnswers] PRIMARY KEY CLUSTERED ([ProductAnswerID] ASC),
    CONSTRAINT [FK_ProductAnswers_ProductQuestions] FOREIGN KEY ([ProductQuestionID]) REFERENCES [dbo].[ProductQuestions] ([ProductQuestionID]),
    CONSTRAINT [FK_ProductAnswers_Vendors] FOREIGN KEY ([VendorID]) REFERENCES [dbo].[Vendors] ([VendorID]),
    CONSTRAINT [FK_ProductAnswers_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID])
);

