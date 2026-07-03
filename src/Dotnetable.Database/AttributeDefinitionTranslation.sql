CREATE TABLE [dbo].[AttributeDefinitionTranslation] (
    [AttributeDefinitionTranslationID] INT            IDENTITY (1, 1) NOT NULL,
    [AttributeDefinitionID]            INT            NOT NULL,
    [LanguageCode]                     CHAR (2)       NOT NULL,
    [Name]                             NVARCHAR (200) NOT NULL,
    [Unit]                             NVARCHAR (30)  NULL,
    CONSTRAINT [PK_AttributeDefinitionTranslation] PRIMARY KEY CLUSTERED ([AttributeDefinitionTranslationID] ASC),
    CONSTRAINT [FK_AttributeDefinitionTranslation_AttributeDefinition] FOREIGN KEY ([AttributeDefinitionID]) REFERENCES [dbo].[AttributeDefinition] ([AttributeDefinitionID])
);

