CREATE TABLE [dbo].[ProductContentSectionTranslations] (
    [ProductContentSectionTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [ProductContentSectionID]            INT            NOT NULL,
    [LanguageCode]                       CHAR (2)       NOT NULL,
    [HtmlContent]                        NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_ProductContentSectionTranslations] PRIMARY KEY CLUSTERED ([ProductContentSectionTranslationID] ASC),
    CONSTRAINT [FK_ProductContentSectionTranslations_ProductContentSections] FOREIGN KEY ([ProductContentSectionID]) REFERENCES [dbo].[ProductContentSections] ([ProductContentSectionID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductContentSectionTranslations_ProductContentSectionID]
    ON [dbo].[ProductContentSectionTranslations] ([ProductContentSectionID] ASC);
