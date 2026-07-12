CREATE TABLE [dbo].[ProductAttributeValues] (
    [ProductAttributeValueID] INT             IDENTITY (1, 1) NOT NULL,
    [ProductID]               INT             NOT NULL,
    [AttributeDefinitionID]   INT             NOT NULL,
    [AttributeOptionID]       INT             NULL,
    [CustomValue]             NVARCHAR (1000) NULL,
    [NumericValue]            DECIMAL (18, 4) NULL,
    [IsFeatured]              BIT             CONSTRAINT [DF_ProductAttributeValues_IsFeatured] DEFAULT ((0)) NOT NULL,
    [SortOrder]               INT             CONSTRAINT [DF_ProductAttributeValues_SortOrder] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_ProductAttributeValues] PRIMARY KEY CLUSTERED ([ProductAttributeValueID] ASC),
    CONSTRAINT [FK_ProductAttributeValues_AttributeDefinitions] FOREIGN KEY ([AttributeDefinitionID]) REFERENCES [dbo].[AttributeDefinitions] ([AttributeDefinitionID]),
    CONSTRAINT [FK_ProductAttributeValues_AttributeOptions] FOREIGN KEY ([AttributeOptionID]) REFERENCES [dbo].[AttributeOptions] ([AttributeOptionID]),
    CONSTRAINT [FK_ProductAttributeValues_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductAttributeValues_AttributeDefinitionID]
    ON [dbo].[ProductAttributeValues] ([AttributeDefinitionID] ASC);

CREATE NONCLUSTERED INDEX [IX_ProductAttributeValues_AttributeOptionID]
    ON [dbo].[ProductAttributeValues] ([AttributeOptionID] ASC);

CREATE NONCLUSTERED INDEX [IX_ProductAttributeValues_ProductID]
    ON [dbo].[ProductAttributeValues] ([ProductID] ASC);
