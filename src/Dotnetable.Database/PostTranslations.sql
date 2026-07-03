CREATE TABLE [dbo].[PostTranslations] (
    [PostTranslationID] INT             IDENTITY (1, 1) NOT NULL,
    [PostID]            INT             NOT NULL,
    [LanguageCode]      CHAR (2)        NOT NULL,
    [Title]             NVARCHAR (300)  NOT NULL,
    [Slug]              NVARCHAR (300)  NOT NULL,
    [Excerpt]           NVARCHAR (1000) NULL,
    [Content]           NVARCHAR (MAX)  NULL,
    CONSTRAINT [PK_PostTranslations] PRIMARY KEY CLUSTERED ([PostTranslationID] ASC),
    CONSTRAINT [FK_PostTranslations_Posts] FOREIGN KEY ([PostID]) REFERENCES [dbo].[Posts] ([PostID])
);

