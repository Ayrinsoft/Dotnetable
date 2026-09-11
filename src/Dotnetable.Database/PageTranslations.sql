CREATE TABLE [dbo].[PageTranslations] (
    [PageTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [PageID]            INT            NOT NULL,
    [LanguageCode]      CHAR (2)       NOT NULL,
    [Title]             NVARCHAR (300) NOT NULL,
    [Slug]              NVARCHAR (300) NOT NULL,
    [Content]           NVARCHAR (MAX) NULL,
    [MetaTitle]         NVARCHAR (300) NULL,
    [MetaDescription]   NVARCHAR (500) NULL,
    [MetaKeywords]      NVARCHAR (500) NULL,
    CONSTRAINT [PK_PageTranslations] PRIMARY KEY CLUSTERED ([PageTranslationID] ASC),
    CONSTRAINT [FK_PageTranslations_Pages] FOREIGN KEY ([PageID]) REFERENCES [dbo].[Pages] ([PageID])
);
GO

CREATE NONCLUSTERED INDEX [IX_PageTranslations_PageID]
    ON [dbo].[PageTranslations] ([PageID] ASC);
