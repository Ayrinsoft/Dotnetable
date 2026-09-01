CREATE TABLE [dbo].[WebsiteSmsSettings] (
    [WebsiteSmsSettingID] INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]           INT             NOT NULL,
    [Provider]            VARCHAR (50)    NOT NULL,
    [Title]               NVARCHAR (150)  NOT NULL,
    [SettingsJSON]        NVARCHAR (4000) NOT NULL,
    [SenderNumber]        VARCHAR (32)    NULL,
    [IsActive]            BIT             NOT NULL,
    [SortOrder]           INT             NOT NULL,
    [CreatedAt]           DATETIME2 (0)   NOT NULL,
    CONSTRAINT [PK_WebsiteSmsSettings] PRIMARY KEY CLUSTERED ([WebsiteSmsSettingID] ASC),
    CONSTRAINT [FK_WebsiteSmsSettings_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


-- One row per SMS gateway a website has registered, mirroring WebsiteStorageSettings.
-- Provider matches ISmsProvider.Key; SettingsJSON holds that provider's own credentials.
-- The active row with the lowest SortOrder sends.
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteSmsSettings_WebsiteID]
    ON [dbo].[WebsiteSmsSettings] ([WebsiteID] ASC);
