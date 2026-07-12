CREATE TABLE [dbo].[ProductAttributeValueTranslations] (
    [ProductAttributeValueTranslationID] INT             IDENTITY (1, 1) NOT NULL,
    [ProductAttributeValueID]            INT             NOT NULL,
    [LanguageCode]                       CHAR (2)        NOT NULL,
    [CustomValue]                        NVARCHAR (1000) NOT NULL,
    CONSTRAINT [PK_ProductAttributeValueTranslations] PRIMARY KEY CLUSTERED ([ProductAttributeValueTranslationID] ASC),
    CONSTRAINT [FK_ProductAttributeValueTranslations_ProductAttributeValues] FOREIGN KEY ([ProductAttributeValueID]) REFERENCES [dbo].[ProductAttributeValues] ([ProductAttributeValueID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductAttributeValueTranslations_ProductAttributeValueID]
    ON [dbo].[ProductAttributeValueTranslations] ([ProductAttributeValueID] ASC);
