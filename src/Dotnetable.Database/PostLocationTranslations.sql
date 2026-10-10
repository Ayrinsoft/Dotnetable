CREATE TABLE [dbo].[PostLocationTranslations] (
    [PostLocationTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [PostID]                    INT            NOT NULL,
    [LanguageCode]              CHAR (2)       NOT NULL,
    [Address]                   NVARCHAR (500) NULL,
    CONSTRAINT [PK_PostLocationTranslations] PRIMARY KEY CLUSTERED ([PostLocationTranslationID] ASC),
    CONSTRAINT [FK_PostLocationTranslations_PostLocations] FOREIGN KEY ([PostID]) REFERENCES [dbo].[PostLocations] ([PostID]) ON DELETE CASCADE
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_PostLocationTranslations_PostID_LanguageCode]
    ON [dbo].[PostLocationTranslations] ([PostID] ASC, [LanguageCode] ASC);
