CREATE TABLE [dbo].[CountryTranslation] (
    [CountryTranslationID] INT           IDENTITY (1, 1) NOT NULL,
    [LanguageCode]         CHAR (2)      NOT NULL,
    [Title]                NVARCHAR (42) NOT NULL,
    [CountryID]            INT           NOT NULL,
    CONSTRAINT [PK_CountryTranslation] PRIMARY KEY CLUSTERED ([CountryTranslationID] ASC),
    CONSTRAINT [FK_CountryTranslation_Country] FOREIGN KEY ([CountryID]) REFERENCES [dbo].[Country] ([CountryID])
);

