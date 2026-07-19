CREATE TABLE [dbo].[EmailTemplateTranslations] (
    [EmailTemplateTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [EmailTemplateID]            INT            NOT NULL,
    [LanguageCode]               CHAR (2)       NOT NULL,
    [Subject]                    NVARCHAR (256) NOT NULL,
    [HtmlBody]                   NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_EmailTemplateTranslations] PRIMARY KEY CLUSTERED ([EmailTemplateTranslationID] ASC),
    CONSTRAINT [FK_EmailTemplateTranslations_EmailTemplates] FOREIGN KEY ([EmailTemplateID]) REFERENCES [dbo].[EmailTemplates] ([EmailTemplateID]) ON DELETE CASCADE
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_EmailTemplateTranslations_EmailTemplateID_LanguageCode]
    ON [dbo].[EmailTemplateTranslations] ([EmailTemplateID] ASC, [LanguageCode] ASC);
