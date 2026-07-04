CREATE TABLE [dbo].[CountryTranslations] (
    [CountryTranslationID] INT           IDENTITY (1, 1) NOT NULL,
    [LanguageCode]         CHAR (2)      NOT NULL,
    [Title]                NVARCHAR (42) NOT NULL,
    [CountryID]            INT           NOT NULL,
    CONSTRAINT [PK_CountryTranslations] PRIMARY KEY CLUSTERED ([CountryTranslationID] ASC),
    CONSTRAINT [FK_CountryTranslations_Countries] FOREIGN KEY ([CountryID]) REFERENCES [dbo].[Countries] ([CountryID])
);

