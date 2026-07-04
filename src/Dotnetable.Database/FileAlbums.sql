CREATE TABLE [dbo].[FileAlbums] (
    [FileAlbumID]  INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]    INT            NOT NULL,
    [Name]         NVARCHAR (120) NOT NULL,
    [Description]  NVARCHAR (400) NULL,
    [CreateDate]   DATETIME       NOT NULL,
    CONSTRAINT [PK_FileAlbums] PRIMARY KEY CLUSTERED ([FileAlbumID] ASC),
    CONSTRAINT [FK_FileAlbums_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
