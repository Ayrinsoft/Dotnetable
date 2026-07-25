CREATE TABLE [dbo].[WebsiteWatermarkSettings] (
    [WebsiteWatermarkSettingID] INT      IDENTITY (1, 1) NOT NULL,
    [WebsiteID]                 INT      NOT NULL,
    [WatermarkFileID]           INT      NULL,
    [Position]                  TINYINT  CONSTRAINT [DF_WebsiteWatermarkSettings_Position] DEFAULT (9) NOT NULL,
    [SizePercent]               INT      CONSTRAINT [DF_WebsiteWatermarkSettings_SizePercent] DEFAULT (20) NOT NULL,
    [Opacity]                   INT      CONSTRAINT [DF_WebsiteWatermarkSettings_Opacity] DEFAULT (80) NOT NULL,
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
