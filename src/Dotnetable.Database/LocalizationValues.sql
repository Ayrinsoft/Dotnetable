CREATE TABLE [dbo].[LocalizationValues] (
    [LocalizationValueID] INT             IDENTITY (1, 1) NOT NULL,
    [LocalizationKeyID]   INT             NOT NULL,
    [ItemValue]           NVARCHAR (2000) NOT NULL,
    [LanguageCode]        CHAR (2)        NOT NULL,
    CONSTRAINT [PK_LocalizationValues] PRIMARY KEY CLUSTERED ([LocalizationValueID] ASC),
    CONSTRAINT [FK_LocalizationValues_LocalizationKeys] FOREIGN KEY ([LocalizationKeyID]) REFERENCES [dbo].[LocalizationKeys] ([LocalizationKeyID])
);
GO

CREATE NONCLUSTERED INDEX [IX_LocalizationValues_LocalizationKeyID]
    ON [dbo].[LocalizationValues] ([LocalizationKeyID] ASC);
