CREATE TABLE [dbo].[CityTranslations] (
    [CityTranslationID] INT           IDENTITY (1, 1) NOT NULL,
    [CityID]            INT           NOT NULL,
    [LanguageCode]      CHAR (2)      NOT NULL,
    [Title]             NVARCHAR (48) NOT NULL,
    CONSTRAINT [PK_CityTranslations] PRIMARY KEY CLUSTERED ([CityTranslationID] ASC),
    CONSTRAINT [FK_CityTranslations_Cities] FOREIGN KEY ([CityID]) REFERENCES [dbo].[Cities] ([CityID])
);

