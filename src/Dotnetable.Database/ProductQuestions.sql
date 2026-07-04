CREATE TABLE [dbo].[ProductQuestions] (
    [ProductQuestionID] INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]         INT             NOT NULL,
    [ProductID]         INT             NOT NULL,
    [WebsiteClientID]   INT             NOT NULL,
    [Body]              NVARCHAR (2000) NOT NULL,
    [Status]            TINYINT         CONSTRAINT [DF_ProductQuestions_Status] DEFAULT ((1)) NOT NULL,
    [CreatedAt]         DATETIME        CONSTRAINT [DF_ProductQuestions_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    [Approved]          BIT             NOT NULL,
    CONSTRAINT [PK_ProductQuestions] PRIMARY KEY CLUSTERED ([ProductQuestionID] ASC),
    CONSTRAINT [FK_ProductQuestions_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID]),
    CONSTRAINT [FK_ProductQuestions_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID]),
    CONSTRAINT [FK_ProductQuestions_WebsiteClient] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClient] ([WebsiteClientID])
);

