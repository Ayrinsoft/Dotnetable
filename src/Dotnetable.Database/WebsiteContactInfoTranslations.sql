CREATE TABLE [dbo].[WebsiteContactInfoTranslations] (
    [WebsiteContactInfoTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteContactInfoID]            INT            NOT NULL,
    [LanguageCode]                    CHAR (2)       NOT NULL,
    [GroupTitle]                      NVARCHAR (64)  NULL,
    [Title]                           NVARCHAR (64)  NOT NULL,
    [Value]                           NVARCHAR (256) NULL,
    CONSTRAINT [PK_WebsiteContactInfoTranslations] PRIMARY KEY CLUSTERED ([WebsiteContactInfoTranslationID] ASC),
    CONSTRAINT [FK_WebsiteContactInfoTranslations_WebsiteContactInfos] FOREIGN KEY ([WebsiteContactInfoID]) REFERENCES [dbo].[WebsiteContactInfos] ([WebsiteContactInfoID])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_WebsiteContactInfoTranslations_WebsiteContactInfoID_LanguageCode]
    ON [dbo].[WebsiteContactInfoTranslations] ([WebsiteContactInfoID] ASC, [LanguageCode] ASC);
