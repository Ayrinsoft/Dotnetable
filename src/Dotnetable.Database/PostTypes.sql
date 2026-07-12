CREATE TABLE [dbo].[PostTypes] (
    [PostTypeID]      INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT            NOT NULL,
    [Name]            NVARCHAR (100) NOT NULL,
    [Slug]            NVARCHAR (100) NOT NULL,
    [HasCategories]   BIT            CONSTRAINT [DF_PostTypes_HasCategories] DEFAULT ((1)) NOT NULL,
    [HasTags]         BIT            CONSTRAINT [DF_PostTypes_HasTags] DEFAULT ((1)) NOT NULL,
    [HasAuthor]       BIT            CONSTRAINT [DF_PostTypes_HasAuthor] DEFAULT ((1)) NOT NULL,
    [CommentsEnabled] BIT            CONSTRAINT [DF_PostTypes_CommentsEnabled] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_PostTypes] PRIMARY KEY CLUSTERED ([PostTypeID] ASC),
    CONSTRAINT [FK_PostTypes_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_PostTypes_WebsiteID]
    ON [dbo].[PostTypes] ([WebsiteID] ASC);
