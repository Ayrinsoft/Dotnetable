CREATE TABLE [dbo].[ProductWarningTranslations] (
    [ProductWarningTranslationID] INT             IDENTITY (1, 1) NOT NULL,
    [ProductWarningID]            INT             NOT NULL,
    [LanguageCode]                CHAR (2)        NOT NULL,
    [Text]                        NVARCHAR (1000) NOT NULL,
    CONSTRAINT [PK_ProductWarningTranslations] PRIMARY KEY CLUSTERED ([ProductWarningTranslationID] ASC),
    CONSTRAINT [FK_ProductWarningTranslations_ProductWarnings] FOREIGN KEY ([ProductWarningID]) REFERENCES [dbo].[ProductWarnings] ([ProductWarningID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductWarningTranslations_ProductWarningID]
    ON [dbo].[ProductWarningTranslations] ([ProductWarningID] ASC);
