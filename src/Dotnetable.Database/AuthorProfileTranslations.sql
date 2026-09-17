CREATE TABLE [dbo].[AuthorProfileTranslations] (
    [AuthorProfileTranslationID] INT             IDENTITY (1, 1) NOT NULL,
    [AuthorProfileID]            INT             NOT NULL,
    [LanguageCode]               CHAR (2)        NOT NULL,
    [DisplayName]                NVARCHAR (150)  NULL,
    [Headline]                   NVARCHAR (200)  NULL,
    [Bio]                        NVARCHAR (1000) NULL,
    [About]                      NVARCHAR (MAX)  NULL,
    CONSTRAINT [PK_AuthorProfileTranslations] PRIMARY KEY CLUSTERED ([AuthorProfileTranslationID] ASC),
    CONSTRAINT [FK_AuthorProfileTranslations_AuthorProfiles] FOREIGN KEY ([AuthorProfileID]) REFERENCES [dbo].[AuthorProfiles] ([AuthorProfileID]) ON DELETE CASCADE
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_AuthorProfileTranslations_AuthorProfileID_LanguageCode]
    ON [dbo].[AuthorProfileTranslations] ([AuthorProfileID] ASC, [LanguageCode] ASC);
