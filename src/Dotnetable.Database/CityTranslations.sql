CREATE TABLE [dbo].[CityTranslation] (
    [CityTranslationID] INT           IDENTITY (1, 1) NOT NULL,
    [CityID]            INT           NOT NULL,
    [LanguageCode]      CHAR (2)      NOT NULL,
    [Title]             NVARCHAR (48) NOT NULL,
    CONSTRAINT [PK_CityTranslation] PRIMARY KEY CLUSTERED ([CityTranslationID] ASC),
    CONSTRAINT [FK_City_Translation_City] FOREIGN KEY ([CityID]) REFERENCES [dbo].[City] ([CityID])
);

