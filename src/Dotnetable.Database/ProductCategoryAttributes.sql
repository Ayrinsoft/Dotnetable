CREATE TABLE [dbo].[ProductCategoryAttributes] (
    [ProductCategoryID]     INT NOT NULL,
    [AttributeDefinitionID] INT NOT NULL,
    [SortOrder]             INT NOT NULL,
    CONSTRAINT [PK_ProductCategoryAttributes] PRIMARY KEY CLUSTERED ([ProductCategoryID] ASC, [AttributeDefinitionID] ASC),
    CONSTRAINT [FK_ProductCategoryAttributes_ProductCategories] FOREIGN KEY ([ProductCategoryID]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryID]),
    CONSTRAINT [FK_ProductCategoryAttributes_AttributeDefinitions] FOREIGN KEY ([AttributeDefinitionID]) REFERENCES [dbo].[AttributeDefinitions] ([AttributeDefinitionID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductCategoryAttributes_AttributeDefinitionID]
    ON [dbo].[ProductCategoryAttributes] ([AttributeDefinitionID] ASC);
