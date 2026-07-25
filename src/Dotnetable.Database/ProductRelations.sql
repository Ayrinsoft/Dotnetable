CREATE TABLE [dbo].[ProductRelations] (
    [ProductID]        INT     NOT NULL,
    [RelatedProductID] INT     NOT NULL,
    [RelationType]     TINYINT NOT NULL,
    [SortOrder]        INT NOT NULL,
    CONSTRAINT [PK_ProductRelations] PRIMARY KEY CLUSTERED ([ProductID] ASC, [RelatedProductID] ASC, [RelationType] ASC),
    CONSTRAINT [FK_ProductRelations_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID]),
    CONSTRAINT [FK_ProductRelations_Products1] FOREIGN KEY ([RelatedProductID]) REFERENCES [dbo].[Products] ([ProductID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductRelations_RelatedProductID]
    ON [dbo].[ProductRelations] ([RelatedProductID] ASC);
