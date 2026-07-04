CREATE TABLE [dbo].[Category] (
    [CategoryID]       INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT            NOT NULL,
    [PostTypeID]       INT            NULL,
    [ParentCategoryID] INT            NULL,
    [Name]             NVARCHAR (200) NOT NULL,
    [Slug]             NVARCHAR (200) NOT NULL,
    [SortOrder]        INT            CONSTRAINT [DF_Category_SortOrder] DEFAULT ((0)) NOT NULL,
    [IsActive]         BIT            CONSTRAINT [DF_Category_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED ([CategoryID] ASC),
    CONSTRAINT [FK_Category_Category] FOREIGN KEY ([ParentCategoryID]) REFERENCES [dbo].[Category] ([CategoryID]),
    CONSTRAINT [FK_Category_PostType] FOREIGN KEY ([PostTypeID]) REFERENCES [dbo].[PostType] ([PostTypeID]),
    CONSTRAINT [FK_Category_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

