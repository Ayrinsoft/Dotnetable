CREATE TABLE [dbo].[TagTranslations] (
    [TagTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [TagID]            INT            NOT NULL,
    [LanguageCode]     CHAR (2)       NOT NULL,
    [Name]             NVARCHAR (150) NOT NULL,
    [Slug]             NVARCHAR (150) NOT NULL,
    CONSTRAINT [PK_TagTranslations] PRIMARY KEY CLUSTERED ([TagTranslationID] ASC),
    CONSTRAINT [FK_TagTranslations_Tags] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tags] ([TagID])
);
GO

CREATE NONCLUSTERED INDEX [IX_TagTranslations_TagID]
    ON [dbo].[TagTranslations] ([TagID] ASC);
