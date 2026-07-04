CREATE TABLE [dbo].[StateTranslations] (
    [StateTranslationID] INT           IDENTITY (1, 1) NOT NULL,
    [Title]              NVARCHAR (48) NOT NULL,
    [LanguageCode]       CHAR (2)      NOT NULL,
    [StateID]            INT           NOT NULL,
    CONSTRAINT [PK_StateTranslations] PRIMARY KEY CLUSTERED ([StateTranslationID] ASC),
    CONSTRAINT [FK_StateTranslations_States] FOREIGN KEY ([StateID]) REFERENCES [dbo].[States] ([StateID])
);

