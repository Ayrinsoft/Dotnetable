CREATE TABLE [dbo].[WebsiteSeoSetting] (
    [WebsiteSeoSettingID]    INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]              INT            NOT NULL,
    [DefaultMetaTitle]       VARCHAR (40)   NULL,
    [TitleSeparator]         CHAR (3)       CONSTRAINT [DF_WebsiteSeoSetting_TitleSeparator] DEFAULT (' | " or " - " or " › ') NULL,
    [DefaultMetaDescription] NVARCHAR (158) NULL,
    [SitemapEnabled]         BIT            NOT NULL,
    [RobotsEnabled]          BIT            NOT NULL,
    [CustomRobotsTxt]        NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_WebsiteSeoSetting] PRIMARY KEY CLUSTERED ([WebsiteSeoSettingID] ASC),
    CONSTRAINT [FK_WebsiteSeoSetting_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Page | {SiteName}', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'WebsiteSeoSetting', @level2type = N'COLUMN', @level2name = N'DefaultMetaTitle';

