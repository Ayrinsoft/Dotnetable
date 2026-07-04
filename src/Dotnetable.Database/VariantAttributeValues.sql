CREATE TABLE [dbo].[VariantAttributeValues] (
    [ProductVariantID]      INT NOT NULL,
    [AttributeDefinitionID] INT NOT NULL,
    [AttributeOptionID]     INT NOT NULL,
    CONSTRAINT [PK_VariantAttributeValues] PRIMARY KEY CLUSTERED ([ProductVariantID] ASC, [AttributeDefinitionID] ASC),
    CONSTRAINT [FK_VariantAttributeValues_AttributeDefinitions] FOREIGN KEY ([AttributeDefinitionID]) REFERENCES [dbo].[AttributeDefinitions] ([AttributeDefinitionID]),
    CONSTRAINT [FK_VariantAttributeValues_AttributeOptions] FOREIGN KEY ([AttributeOptionID]) REFERENCES [dbo].[AttributeOptions] ([AttributeOptionID]),
    CONSTRAINT [FK_VariantAttributeValues_ProductVariants] FOREIGN KEY ([ProductVariantID]) REFERENCES [dbo].[ProductVariants] ([ProductVariantID])
);

