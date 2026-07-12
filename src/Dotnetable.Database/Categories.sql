CREATE TABLE [dbo].[Categories] (
    [CategoryID]       INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT            NOT NULL,
    [PostTypeID]       INT            NULL,
    [ParentCategoryID] INT            NULL,
    [Name]             NVARCHAR (200) NOT NULL,
    [Slug]             NVARCHAR (200) NOT NULL,
    [SortOrder]        INT            CONSTRAINT [DF_Categories_SortOrder] DEFAULT ((0)) NOT NULL,
    [IsActive]         BIT            CONSTRAINT [DF_Categories_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([CategoryID] ASC),
    CONSTRAINT [FK_Categories_Categories] FOREIGN KEY ([ParentCategoryID]) REFERENCES [dbo].[Categories] ([CategoryID]),
    CONSTRAINT [FK_Categories_PostTypes] FOREIGN KEY ([PostTypeID]) REFERENCES [dbo].[PostTypes] ([PostTypeID]),
    CONSTRAINT [FK_Categories_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_Categories_ParentCategoryID]
    ON [dbo].[Categories] ([ParentCategoryID] ASC);

CREATE NONCLUSTERED INDEX [IX_Categories_PostTypeID]
    ON [dbo].[Categories] ([PostTypeID] ASC);

CREATE NONCLUSTERED INDEX [IX_Categories_WebsiteID]
    ON [dbo].[Categories] ([WebsiteID] ASC);
