CREATE TABLE [dbo].[LocalizationKey] (
    [LocalizationKeyID] INT             IDENTITY (1, 1) NOT NULL,
    [ItemKey]           VARCHAR (72)    NOT NULL,
    [DefaultValue]      NVARCHAR (2000) NOT NULL,
    [WebsiteID]         INT             NOT NULL,
    CONSTRAINT [PK_LocalizationKey] PRIMARY KEY CLUSTERED ([LocalizationKeyID] ASC),
    CONSTRAINT [FK_LocalizationKey_Language] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Language] ([LangaugeID])
);

