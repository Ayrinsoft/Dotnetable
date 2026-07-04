CREATE TABLE [dbo].[WebstieStorageSettings] (
    [WebsiteStorageSettingsID] INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]                INT             NOT NULL,
    [StorageProvider]          SMALLINT        NOT NULL,
    [StorageSettingsJSON]      NVARCHAR (2000) NOT NULL,
    [Active]                   BIT             NOT NULL,
    [MaxFileSizeKB]            BIGINT          NOT NULL,
    [AllowedExtensions]        VARCHAR (710)   NULL,
    [AutoGenerateThumbnails]   BIT             NOT NULL,
    CONSTRAINT [PK_WebstieStorageSettings] PRIMARY KEY CLUSTERED ([WebsiteStorageSettingsID] ASC),
    CONSTRAINT [FK_WebstieStorageSettings_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

