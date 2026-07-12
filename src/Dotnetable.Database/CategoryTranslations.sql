CREATE TABLE [dbo].[CategoryTranslations] (
    [CategoryTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [CategoryID]            INT            NOT NULL,
    [LanguageCode]          CHAR (2)       NOT NULL,
    [Name]                  NVARCHAR (200) NOT NULL,
    [Slug]                  NVARCHAR (200) NOT NULL,
    CONSTRAINT [PK_CategoryTranslations] PRIMARY KEY CLUSTERED ([CategoryTranslationID] ASC),
    CONSTRAINT [FK_CategoryTranslations_Categories] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Categories] ([CategoryID])
);
GO

CREATE NONCLUSTERED INDEX [IX_CategoryTranslations_CategoryID]
    ON [dbo].[CategoryTranslations] ([CategoryID] ASC);
