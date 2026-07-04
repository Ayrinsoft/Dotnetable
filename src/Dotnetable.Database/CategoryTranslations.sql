CREATE TABLE [dbo].[CategoryTranslation] (
    [CategoryTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [CategoryID]            INT            NOT NULL,
    [LanguageCode]          CHAR (2)       NOT NULL,
    [Name]                  NVARCHAR (200) NOT NULL,
    [Slug]                  NVARCHAR (200) NOT NULL,
    CONSTRAINT [PK_CategoryTranslation] PRIMARY KEY CLUSTERED ([CategoryTranslationID] ASC),
    CONSTRAINT [FK_CategoryTranslation_Category] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Category] ([CategoryID])
);

