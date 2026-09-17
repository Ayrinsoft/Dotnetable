CREATE TABLE [dbo].[AuthorResumeItemTranslations] (
    [AuthorResumeItemTranslationID] INT             IDENTITY (1, 1) NOT NULL,
    [AuthorResumeItemID]            INT             NOT NULL,
    [LanguageCode]                  CHAR (2)        NOT NULL,
    [Title]                         NVARCHAR (200)  NULL,
    [Organization]                  NVARCHAR (200)  NULL,
    [Location]                      NVARCHAR (150)  NULL,
    [Description]                   NVARCHAR (4000) NULL,
    CONSTRAINT [PK_AuthorResumeItemTranslations] PRIMARY KEY CLUSTERED ([AuthorResumeItemTranslationID] ASC),
    CONSTRAINT [FK_AuthorResumeItemTranslations_AuthorResumeItems] FOREIGN KEY ([AuthorResumeItemID]) REFERENCES [dbo].[AuthorResumeItems] ([AuthorResumeItemID]) ON DELETE CASCADE
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_AuthorResumeItemTranslations_AuthorResumeItemID_LanguageCode]
    ON [dbo].[AuthorResumeItemTranslations] ([AuthorResumeItemID] ASC, [LanguageCode] ASC);
