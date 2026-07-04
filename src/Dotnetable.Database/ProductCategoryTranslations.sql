CREATE TABLE [dbo].[ProductCategoryTranslations] (
    [ProductCategoryTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [ProductCategoryID]            INT            NOT NULL,
    [LanguageCode]                 CHAR (2)       NOT NULL,
    [Name]                         NVARCHAR (200) NOT NULL,
    [Slug]                         NVARCHAR (200) NOT NULL,
    CONSTRAINT [PK_ProductCategoryTranslations] PRIMARY KEY CLUSTERED ([ProductCategoryTranslationID] ASC),
    CONSTRAINT [FK_ProductCategoryTranslations_ProductCategories] FOREIGN KEY ([ProductCategoryID]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryID])
);

