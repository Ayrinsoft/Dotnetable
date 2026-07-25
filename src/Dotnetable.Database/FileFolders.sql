CREATE TABLE [dbo].[FileFolders] (
    [FileFolderID]    INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT            NOT NULL,
    [ParentFolderID]  INT            NULL,
    [Name]            NVARCHAR (120) NOT NULL,
    [Description]     NVARCHAR (400) NULL,
    [CreateDate]      DATETIME       NOT NULL,
    CONSTRAINT [PK_FileFolders] PRIMARY KEY CLUSTERED ([FileFolderID] ASC),
    CONSTRAINT [FK_FileFolders_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_FileFolders_Parent] FOREIGN KEY ([ParentFolderID]) REFERENCES [dbo].[FileFolders] ([FileFolderID])
);
GO

CREATE NONCLUSTERED INDEX [IX_FileFolders_WebsiteID]
    ON [dbo].[FileFolders] ([WebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_FileFolders_ParentFolderID]
    ON [dbo].[FileFolders] ([ParentFolderID] ASC);
