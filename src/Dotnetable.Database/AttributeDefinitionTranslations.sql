CREATE TABLE [dbo].[AttributeDefinitionTranslations] (
    [AttributeDefinitionTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [AttributeDefinitionID]            INT            NOT NULL,
    [LanguageCode]                     CHAR (2)       NOT NULL,
    [Name]                             NVARCHAR (200) NOT NULL,
    [Unit]                             NVARCHAR (30)  NULL,
    CONSTRAINT [PK_AttributeDefinitionTranslations] PRIMARY KEY CLUSTERED ([AttributeDefinitionTranslationID] ASC),
    CONSTRAINT [FK_AttributeDefinitionTranslations_AttributeDefinitions] FOREIGN KEY ([AttributeDefinitionID]) REFERENCES [dbo].[AttributeDefinitions] ([AttributeDefinitionID])
);
GO

CREATE NONCLUSTERED INDEX [IX_AttributeDefinitionTranslations_AttributeDefinitionID]
    ON [dbo].[AttributeDefinitionTranslations] ([AttributeDefinitionID] ASC);
