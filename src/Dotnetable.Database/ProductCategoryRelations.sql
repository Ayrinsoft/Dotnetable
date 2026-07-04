CREATE TABLE [dbo].[ProductCategoryRelations] (
    [ProductID]                INT     NOT NULL,
    [RelatedProductCategoryID] INT     NOT NULL,
    [RelationType]             TINYINT NOT NULL,
    [MaxItems]                 INT     CONSTRAINT [DF_ProductCategoryRelations_MaxItems] DEFAULT ((10)) NOT NULL,
    CONSTRAINT [PK_ProductCategoryRelations] PRIMARY KEY CLUSTERED ([ProductID] ASC, [RelatedProductCategoryID] ASC, [RelationType] ASC),
    CONSTRAINT [FK_ProductCategoryRelations_ProductCategories] FOREIGN KEY ([RelatedProductCategoryID]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryID]),
    CONSTRAINT [FK_ProductCategoryRelations_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID])
);

