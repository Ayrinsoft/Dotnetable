CREATE TABLE [dbo].[BrandTranslations] (
    [BrandTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [BrandID]            INT            NOT NULL,
    [LanguageCode]       CHAR (2)       NOT NULL,
    [Name]               NVARCHAR (200) NOT NULL,
    [Slug]               NVARCHAR (200) NOT NULL,
    CONSTRAINT [PK_BrandTranslations] PRIMARY KEY CLUSTERED ([BrandTranslationID] ASC),
    CONSTRAINT [FK_BrandTranslations_Brands] FOREIGN KEY ([BrandID]) REFERENCES [dbo].[Brands] ([BrandID])
);

