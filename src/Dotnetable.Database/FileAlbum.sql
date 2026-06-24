CREATE TABLE [dbo].[FileAlbum] (
    [FileAlbumID]  INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]    INT            NOT NULL,
    [Name]         NVARCHAR (120) NOT NULL,
    [Description]  NVARCHAR (400) NULL,
    [CreateDate]   DATETIME       NOT NULL,
    CONSTRAINT [PK_FileAlbum] PRIMARY KEY CLUSTERED ([FileAlbumID] ASC),
    CONSTRAINT [FK_FileAlbum_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);
