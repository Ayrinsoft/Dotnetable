CREATE TABLE [dbo].[ProductCategoryMaps] (
    [ProductID]         INT NOT NULL,
    [ProductCategoryID] INT NOT NULL,
    [IsPrimary]         BIT NOT NULL,
    CONSTRAINT [PK_ProductCategoryMaps] PRIMARY KEY CLUSTERED ([ProductID] ASC, [ProductCategoryID] ASC),
    CONSTRAINT [FK_ProductCategoryMaps_ProductCategories] FOREIGN KEY ([ProductCategoryID]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryID]),
    CONSTRAINT [FK_ProductCategoryMaps_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductCategoryMaps_ProductCategoryID]
    ON [dbo].[ProductCategoryMaps] ([ProductCategoryID] ASC);
