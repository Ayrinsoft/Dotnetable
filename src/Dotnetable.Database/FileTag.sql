CREATE TABLE [dbo].[FileTag] (
    [FileTagID]  INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]  INT           NOT NULL,
    [Name]       NVARCHAR (60) NOT NULL,
    CONSTRAINT [PK_FileTag] PRIMARY KEY CLUSTERED ([FileTagID] ASC),
    CONSTRAINT [FK_FileTag_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);
