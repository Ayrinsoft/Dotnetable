CREATE TABLE [dbo].[WebsiteWhatsAppSettings] (
    [WebsiteWhatsAppSettingID] INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]                INT             NOT NULL,
    [Provider]                 VARCHAR (50)    NOT NULL,
    [Title]                    NVARCHAR (150)  NOT NULL,
    [SettingsJSON]             NVARCHAR (4000) NOT NULL,
    [SenderNumber]             VARCHAR (32)    NULL,
    [IsActive]                 BIT             NOT NULL,
    [SortOrder]                INT             NOT NULL,
    [CreatedAt]                DATETIME2 (0)   NOT NULL,
    CONSTRAINT [PK_WebsiteWhatsAppSettings] PRIMARY KEY CLUSTERED ([WebsiteWhatsAppSettingID] ASC),
    CONSTRAINT [FK_WebsiteWhatsAppSettings_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


-- One row per WhatsApp gateway a website has registered, mirroring WebsiteSmsSettings.
-- Provider matches IWhatsAppProvider.Key; SettingsJSON holds that provider's own credentials.
-- The active row with the lowest SortOrder sends; a website with none falls back to the master website's.
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteWhatsAppSettings_WebsiteID]
    ON [dbo].[WebsiteWhatsAppSettings] ([WebsiteID] ASC);
