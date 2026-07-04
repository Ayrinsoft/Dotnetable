CREATE TABLE [dbo].[LocalizationKeys] (
    [LocalizationKeyID] INT             IDENTITY (1, 1) NOT NULL,
    [ItemKey]           VARCHAR (72)    NOT NULL,
    [DefaultValue]      NVARCHAR (2000) NOT NULL,
    [WebsiteID]         INT             NOT NULL,
    CONSTRAINT [PK_LocalizationKeys] PRIMARY KEY CLUSTERED ([LocalizationKeyID] ASC),
    CONSTRAINT [FK_LocalizationKeys_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

