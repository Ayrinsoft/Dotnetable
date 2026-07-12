CREATE TABLE [dbo].[FileTags] (
    [FileTagID]  INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]  INT           NOT NULL,
    [Name]       NVARCHAR (60) NOT NULL,
    CONSTRAINT [PK_FileTags] PRIMARY KEY CLUSTERED ([FileTagID] ASC),
    CONSTRAINT [FK_FileTags_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_FileTags_WebsiteID]
    ON [dbo].[FileTags] ([WebsiteID] ASC);
