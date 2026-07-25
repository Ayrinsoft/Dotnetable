CREATE TABLE [dbo].[ProductQuestions] (
    [ProductQuestionID] INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]         INT             NOT NULL,
    [ProductID]         INT             NOT NULL,
    [WebsiteClientID]   INT             NOT NULL,
    [Body]              NVARCHAR (2000) NOT NULL,
    [Status]            TINYINT NOT NULL,
    [CreatedAt]         DATETIME NOT NULL,
    [Approved]          BIT             NOT NULL,
    CONSTRAINT [PK_ProductQuestions] PRIMARY KEY CLUSTERED ([ProductQuestionID] ASC),
    CONSTRAINT [FK_ProductQuestions_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID]),
    CONSTRAINT [FK_ProductQuestions_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_ProductQuestions_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductQuestions_ProductID]
    ON [dbo].[ProductQuestions] ([ProductID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ProductQuestions_WebsiteClientID]
    ON [dbo].[ProductQuestions] ([WebsiteClientID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ProductQuestions_WebsiteID]
    ON [dbo].[ProductQuestions] ([WebsiteID] ASC);
