CREATE TABLE [dbo].[PostTranslations] (
    [PostTranslationID] INT             IDENTITY (1, 1) NOT NULL,
    [PostID]            INT             NOT NULL,
    [LanguageCode]      CHAR (2)        NOT NULL,
    [Title]             NVARCHAR (300)  NOT NULL,
    [Slug]              NVARCHAR (300)  NOT NULL,
    [Excerpt]           NVARCHAR (1000) NULL,
    [Content]           NVARCHAR (MAX)  NULL,
    [MetaTitle]         NVARCHAR (300)  NULL,
    [MetaDescription]   NVARCHAR (500)  NULL,
    [MetaKeywords]      NVARCHAR (500)  NULL,
    CONSTRAINT [PK_PostTranslations] PRIMARY KEY CLUSTERED ([PostTranslationID] ASC),
    CONSTRAINT [FK_PostTranslations_Posts] FOREIGN KEY ([PostID]) REFERENCES [dbo].[Posts] ([PostID])
);
GO

CREATE NONCLUSTERED INDEX [IX_PostTranslations_PostID]
    ON [dbo].[PostTranslations] ([PostID] ASC);
