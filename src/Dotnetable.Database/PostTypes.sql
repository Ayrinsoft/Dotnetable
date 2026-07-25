CREATE TABLE [dbo].[PostTypes] (
    [PostTypeID]      INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT            NOT NULL,
    [Name]            NVARCHAR (100) NOT NULL,
    [Slug]            NVARCHAR (100) NOT NULL,
    [HasCategories]   BIT NOT NULL,
    [HasTags]         BIT NOT NULL,
    [HasAuthor]       BIT NOT NULL,
    [CommentsEnabled] BIT NOT NULL,
    CONSTRAINT [PK_PostTypes] PRIMARY KEY CLUSTERED ([PostTypeID] ASC),
    CONSTRAINT [FK_PostTypes_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_PostTypes_WebsiteID]
    ON [dbo].[PostTypes] ([WebsiteID] ASC);
