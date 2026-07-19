CREATE TABLE [dbo].[LocalizationKeys] (
    [LocalizationKeyID] INT             IDENTITY (1, 1) NOT NULL,
    [ItemKey]           VARCHAR (72)    NOT NULL,
    [DefaultValue]      NVARCHAR (2000) NOT NULL,
    -- NULL = admin panel UI strings. Non-null = that website's storefront/theme keys only.
    [WebsiteID]         INT             NULL,
    CONSTRAINT [PK_LocalizationKeys] PRIMARY KEY CLUSTERED ([LocalizationKeyID] ASC),
    CONSTRAINT [FK_LocalizationKeys_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_LocalizationKeys_WebsiteID]
    ON [dbo].[LocalizationKeys] ([WebsiteID] ASC);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_LocalizationKeys_Admin_ItemKey]
    ON [dbo].[LocalizationKeys] ([ItemKey] ASC)
    WHERE [WebsiteID] IS NULL;
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_LocalizationKeys_WebsiteID_ItemKey]
    ON [dbo].[LocalizationKeys] ([WebsiteID] ASC, [ItemKey] ASC)
    WHERE [WebsiteID] IS NOT NULL;
