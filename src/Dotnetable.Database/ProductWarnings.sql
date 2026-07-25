CREATE TABLE [dbo].[ProductWarnings] (
    [ProductWarningID] INT             IDENTITY (1, 1) NOT NULL,
    [ProductID]        INT             NOT NULL,
    [Severity]         NVARCHAR (20) NOT NULL,
    [Text]             NVARCHAR (1000) NOT NULL,
    [IsActive]         BIT NOT NULL,
    CONSTRAINT [PK_ProductWarnings] PRIMARY KEY CLUSTERED ([ProductWarningID] ASC),
    CONSTRAINT [FK_ProductWarnings_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductWarnings_ProductID]
    ON [dbo].[ProductWarnings] ([ProductID] ASC);
