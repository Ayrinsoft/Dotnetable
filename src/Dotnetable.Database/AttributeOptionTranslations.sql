CREATE TABLE [dbo].[AttributeOptionTranslation] (
    [AttributeOptionTranslationID] INT             IDENTITY (1, 1) NOT NULL,
    [AttributeOptionID]            INT             NOT NULL,
    [LanguageCode]                 CHAR (2)        NOT NULL,
    [Value]                        NVARCHAR (2000) NOT NULL,
    CONSTRAINT [PK_AttributeOptionTranslation] PRIMARY KEY CLUSTERED ([AttributeOptionTranslationID] ASC),
    CONSTRAINT [FK_AttributeOptionTranslation_AttributeOption] FOREIGN KEY ([AttributeOptionID]) REFERENCES [dbo].[AttributeOption] ([AttributeOptionID])
);

