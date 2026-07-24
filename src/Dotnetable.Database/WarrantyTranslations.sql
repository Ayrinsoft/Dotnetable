CREATE TABLE [dbo].[WarrantyTranslations] (
    [WarrantyTranslationID] INT             IDENTITY (1, 1) NOT NULL,
    [WarrantyID]            INT             NOT NULL,
    [LanguageCode]          CHAR (2)        NOT NULL,
    [Title]                 NVARCHAR (200)  NOT NULL,
    [Description]           NVARCHAR (2000) NULL,
    [ProviderName]          NVARCHAR (200)  NULL,
    CONSTRAINT [PK_WarrantyTranslations] PRIMARY KEY CLUSTERED ([WarrantyTranslationID] ASC),
    CONSTRAINT [FK_WarrantyTranslations_Warranties] FOREIGN KEY ([WarrantyID]) REFERENCES [dbo].[Warranties] ([WarrantyID])
);
GO

CREATE NONCLUSTERED INDEX [IX_WarrantyTranslations_WarrantyID]
    ON [dbo].[WarrantyTranslations] ([WarrantyID] ASC);
