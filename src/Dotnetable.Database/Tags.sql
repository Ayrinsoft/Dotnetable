CREATE TABLE [dbo].[Tags] (
    [TagID]     INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID] INT            NOT NULL,
    [Name]      NVARCHAR (150) NOT NULL,
    [Slug]      NVARCHAR (150) NOT NULL,
    CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED ([TagID] ASC),
    CONSTRAINT [FK_Tags_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_Tags_WebsiteID]
    ON [dbo].[Tags] ([WebsiteID] ASC);
