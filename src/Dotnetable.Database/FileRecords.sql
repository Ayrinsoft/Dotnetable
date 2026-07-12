CREATE TABLE [dbo].[FileRecords] (
    [FileRecordID]             INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteStorageSettingsID] INT             NOT NULL,
    [StorageProvider]          SMALLINT        NOT NULL,
    [StoragePath]              NVARCHAR (350)  NULL,
    [CNDUrl]                   NVARCHAR (450)  NULL,
    [OriginalFileName]         NVARCHAR (120)  NOT NULL,
    [StoredFileName]           VARCHAR (40)    NOT NULL,
    [MimeType]                 VARCHAR (74)    NOT NULL,
    [FileSizeKB]               INT             NOT NULL,
    [MetadataJSON]             NVARCHAR (2000) NULL,
    [AltText]                  NVARCHAR (120)  NULL,
    [Title]                    NVARCHAR (50)   NULL,
    [IsDeleted]                BIT             NOT NULL,
    [UploadDate]               DATETIME        NOT NULL,
    [ThumbnailStorage]         NVARCHAR (350)  NULL,
    [ThumbnailCDN]             NVARCHAR (450)  NULL,
    [FileCategory]             TINYINT         NOT NULL,
    [CDNFileCode]              VARCHAR (80)    NULL,
    [FileAlbumID]              INT             NULL,
    [WebsiteID]                INT             NOT NULL,
    [UploaderMemberID]         INT             NULL,
    [WebsiteClientID]          INT             NULL,
    CONSTRAINT [PK_FileRecords] PRIMARY KEY CLUSTERED ([FileRecordID] ASC),
    CONSTRAINT [FK_FileRecords_FileAlbums] FOREIGN KEY ([FileAlbumID]) REFERENCES [dbo].[FileAlbums] ([FileAlbumID]),
    CONSTRAINT [FK_FileRecords_Members] FOREIGN KEY ([UploaderMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_FileRecords_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_FileRecords_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID]),
    CONSTRAINT [FK_FileRecords_WebsiteStorageSettings] FOREIGN KEY ([WebsiteStorageSettingsID]) REFERENCES [dbo].[WebsiteStorageSettings] ([WebsiteStorageSettingsID])
);
GO

CREATE NONCLUSTERED INDEX [IX_FileRecords_FileAlbumID]
    ON [dbo].[FileRecords] ([FileAlbumID] ASC);

CREATE NONCLUSTERED INDEX [IX_FileRecords_UploaderMemberID]
    ON [dbo].[FileRecords] ([UploaderMemberID] ASC);

CREATE NONCLUSTERED INDEX [IX_FileRecords_WebsiteClientID]
    ON [dbo].[FileRecords] ([WebsiteClientID] ASC);

CREATE NONCLUSTERED INDEX [IX_FileRecords_WebsiteID]
    ON [dbo].[FileRecords] ([WebsiteID] ASC);

CREATE NONCLUSTERED INDEX [IX_FileRecords_WebsiteStorageSettingsID]
    ON [dbo].[FileRecords] ([WebsiteStorageSettingsID] ASC);
