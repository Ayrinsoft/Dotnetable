CREATE TABLE [dbo].[ProductCategoryRelations] (
    [ProductID]                INT     NOT NULL,
    [RelatedProductCategoryID] INT     NOT NULL,
    [RelationType]             TINYINT NOT NULL,
    [MaxItems]                 INT NOT NULL,
    CONSTRAINT [PK_ProductCategoryRelations] PRIMARY KEY CLUSTERED ([ProductID] ASC, [RelatedProductCategoryID] ASC, [RelationType] ASC),
    CONSTRAINT [FK_ProductCategoryRelations_ProductCategories] FOREIGN KEY ([RelatedProductCategoryID]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryID]),
    CONSTRAINT [FK_ProductCategoryRelations_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductCategoryRelations_RelatedProductCategoryID]
    ON [dbo].[ProductCategoryRelations] ([RelatedProductCategoryID] ASC);
