CREATE TABLE [dbo].[WebsiteStorageSettings] (
    [WebsiteStorageSettingsID] INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]                INT             NOT NULL,
    [StorageProvider]          SMALLINT        NOT NULL,
    [StorageSettingsJSON]      NVARCHAR (2000) NOT NULL,
    [Active]                   BIT             NOT NULL,
    [MaxFileSizeKB]            BIGINT          NOT NULL,
    [AllowedExtensions]        VARCHAR (710)   NULL,
    [AutoGenerateThumbnails]   BIT             NOT NULL,
    CONSTRAINT [PK_WebsiteStorageSettings] PRIMARY KEY CLUSTERED ([WebsiteStorageSettingsID] ASC),
    CONSTRAINT [FK_WebsiteStorageSettings_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteStorageSettings_WebsiteID]
    ON [dbo].[WebsiteStorageSettings] ([WebsiteID] ASC);
