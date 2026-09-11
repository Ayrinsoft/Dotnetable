CREATE TABLE [dbo].[AdvertisementTranslations] (
    [AdvertisementTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [AdvertisementID]            INT            NOT NULL,
    [LanguageCode]               CHAR (2)       NOT NULL,
    [Keyword]                    NVARCHAR (200) NOT NULL,
    [Url]                        NVARCHAR (500) NULL,
    CONSTRAINT [PK_AdvertisementTranslations] PRIMARY KEY CLUSTERED ([AdvertisementTranslationID] ASC),
    CONSTRAINT [FK_AdvertisementTranslations_Advertisements] FOREIGN KEY ([AdvertisementID]) REFERENCES [dbo].[Advertisements] ([AdvertisementID])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_AdvertisementTranslations_AdvertisementID_LanguageCode]
    ON [dbo].[AdvertisementTranslations] ([AdvertisementID] ASC, [LanguageCode] ASC);
