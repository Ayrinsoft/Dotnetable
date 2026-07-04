CREATE TABLE [dbo].[PostType] (
    [PostTypeID]      INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT            NOT NULL,
    [Name]            NVARCHAR (100) NOT NULL,
    [Slug]            NVARCHAR (100) NOT NULL,
    [HasCategories]   BIT            CONSTRAINT [DF_PostType_HasCategories] DEFAULT ((1)) NOT NULL,
    [HasTags]         BIT            CONSTRAINT [DF_PostType_HasTags] DEFAULT ((1)) NOT NULL,
    [HasAuthor]       BIT            CONSTRAINT [DF_PostType_HasAuthor] DEFAULT ((1)) NOT NULL,
    [CommentsEnabled] BIT            CONSTRAINT [DF_PostType_CommentsEnabled] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_PostType] PRIMARY KEY CLUSTERED ([PostTypeID] ASC),
    CONSTRAINT [FK_PostType_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

