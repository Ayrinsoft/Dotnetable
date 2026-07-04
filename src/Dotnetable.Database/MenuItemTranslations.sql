CREATE TABLE [dbo].[MenuItemTranslations] (
    [MenuItemTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [MenuItemID]            INT            NOT NULL,
    [LanguageCode]          CHAR (2)       NOT NULL,
    [Title]                 NVARCHAR (200) NOT NULL,
    CONSTRAINT [PK_MenuItemTranslations] PRIMARY KEY CLUSTERED ([MenuItemTranslationID] ASC),
    CONSTRAINT [FK_MenuItemTranslations_MenuItems] FOREIGN KEY ([MenuItemID]) REFERENCES [dbo].[MenuItems] ([MenuItemID])
);

