CREATE TABLE [dbo].[WebsiteSeoSettings] (
    [WebsiteSeoSettingID]    INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]              INT            NOT NULL,
    [DefaultMetaTitle]       VARCHAR (40)   NULL,
    [TitleSeparator]         CHAR (3) NULL,
    [DefaultMetaDescription] NVARCHAR (158) NULL,
    [SitemapEnabled]         BIT            NOT NULL,
    [RobotsEnabled]          BIT            NOT NULL,
    [CustomRobotsTxt]        NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_WebsiteSeoSettings] PRIMARY KEY CLUSTERED ([WebsiteSeoSettingID] ASC),
    CONSTRAINT [FK_WebsiteSeoSettings_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Page | {SiteName}', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'WebsiteSeoSettings', @level2type = N'COLUMN', @level2name = N'DefaultMetaTitle';
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteSeoSettings_WebsiteID]
    ON [dbo].[WebsiteSeoSettings] ([WebsiteID] ASC);
