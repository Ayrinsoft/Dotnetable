CREATE TABLE [dbo].[VariantAttributeValues] (
    [ProductVariantID]      INT NOT NULL,
    [AttributeDefinitionID] INT NOT NULL,
    [AttributeOptionID]     INT NOT NULL,
    CONSTRAINT [PK_VariantAttributeValues] PRIMARY KEY CLUSTERED ([ProductVariantID] ASC, [AttributeDefinitionID] ASC),
    CONSTRAINT [FK_VariantAttributeValues_AttributeDefinition] FOREIGN KEY ([AttributeDefinitionID]) REFERENCES [dbo].[AttributeDefinition] ([AttributeDefinitionID]),
    CONSTRAINT [FK_VariantAttributeValues_AttributeOption] FOREIGN KEY ([AttributeOptionID]) REFERENCES [dbo].[AttributeOption] ([AttributeOptionID]),
    CONSTRAINT [FK_VariantAttributeValues_ProductVariants] FOREIGN KEY ([ProductVariantID]) REFERENCES [dbo].[ProductVariants] ([ProductVariantID])
);

