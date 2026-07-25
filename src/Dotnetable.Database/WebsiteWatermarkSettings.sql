CREATE TABLE [dbo].[WebsiteWatermarkSettings] (
    [WebsiteWatermarkSettingID] INT      IDENTITY (1, 1) NOT NULL,
    [WebsiteID]                 INT      NOT NULL,
    [WatermarkFileID]           INT      NULL,
    [Position]                  TINYINT NOT NULL,
    [SizePercent]               INT NOT NULL,
    [Opacity]                   INT NOT NULL,
    [Enabled]                   BIT      NOT NULL,
    CONSTRAINT [PK_WebsiteWatermarkSettings] PRIMARY KEY CLUSTERED ([WebsiteWatermarkSettingID] ASC),
    CONSTRAINT [FK_WebsiteWatermarkSettings_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_WebsiteWatermarkSettings_FileRecords] FOREIGN KEY ([WatermarkFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID])
);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteWatermarkSettings_WatermarkFileID]
    ON [dbo].[WebsiteWatermarkSettings] ([WatermarkFileID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteWatermarkSettings_WebsiteID]
    ON [dbo].[WebsiteWatermarkSettings] ([WebsiteID] ASC);
