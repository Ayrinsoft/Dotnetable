CREATE TABLE [dbo].[MenuItemTranslation] (
    [MenuItemTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [MenuItemID]            INT            NOT NULL,
    [LanguageCode]          CHAR (2)       NOT NULL,
    [Title]                 NVARCHAR (200) NOT NULL,
    CONSTRAINT [PK_MenuItemTranslation] PRIMARY KEY CLUSTERED ([MenuItemTranslationID] ASC),
    CONSTRAINT [FK_MenuItemTranslation_MenuItem] FOREIGN KEY ([MenuItemID]) REFERENCES [dbo].[MenuItem] ([MenuItemID])
);

