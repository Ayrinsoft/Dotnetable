-- Category ↔ attribute mapping for product-level attributes (e.g. TV → screen size).
-- Safe additive migration for SQL Server SSDT / manual apply.

IF OBJECT_ID(N'dbo.ProductCategoryAttributes', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductCategoryAttributes] (
        [ProductCategoryID]     INT NOT NULL,
        [AttributeDefinitionID] INT NOT NULL,
        [SortOrder]             INT NOT NULL,
        CONSTRAINT [PK_ProductCategoryAttributes] PRIMARY KEY CLUSTERED ([ProductCategoryID] ASC, [AttributeDefinitionID] ASC),
        CONSTRAINT [FK_ProductCategoryAttributes_ProductCategories] FOREIGN KEY ([ProductCategoryID]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryID]),
        CONSTRAINT [FK_ProductCategoryAttributes_AttributeDefinitions] FOREIGN KEY ([AttributeDefinitionID]) REFERENCES [dbo].[AttributeDefinitions] ([AttributeDefinitionID])
    );

    CREATE NONCLUSTERED INDEX [IX_ProductCategoryAttributes_AttributeDefinitionID]
        ON [dbo].[ProductCategoryAttributes] ([AttributeDefinitionID] ASC);
END
GO
