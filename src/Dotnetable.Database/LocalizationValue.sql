CREATE TABLE [dbo].[LocalizationValue] (
    [LocalizationValueID] INT             IDENTITY (1, 1) NOT NULL,
    [LocalizationKeyID]   INT             NOT NULL,
    [ItemValue]           NVARCHAR (2000) NOT NULL,
    [LanguageCode]        CHAR (2)        NOT NULL,
    CONSTRAINT [PK_LocalizationValue] PRIMARY KEY CLUSTERED ([LocalizationValueID] ASC),
    CONSTRAINT [FK_LocalizationValue_LocalizationKey] FOREIGN KEY ([LocalizationKeyID]) REFERENCES [dbo].[LocalizationKey] ([LocalizationKeyID])
);

