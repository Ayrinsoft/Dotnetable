CREATE TABLE [dbo].[AttributeOptionTranslations] (
    [AttributeOptionTranslationID] INT             IDENTITY (1, 1) NOT NULL,
    [AttributeOptionID]            INT             NOT NULL,
    [LanguageCode]                 CHAR (2)        NOT NULL,
    [Value]                        NVARCHAR (2000) NOT NULL,
    CONSTRAINT [PK_AttributeOptionTranslations] PRIMARY KEY CLUSTERED ([AttributeOptionTranslationID] ASC),
    CONSTRAINT [FK_AttributeOptionTranslations_AttributeOptions] FOREIGN KEY ([AttributeOptionID]) REFERENCES [dbo].[AttributeOptions] ([AttributeOptionID])
);

