CREATE TABLE [dbo].[ProductCategories] (
    [ProductCategoryID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]         INT            NOT NULL,
    [ParentCategoryID]  INT            NULL,
    [Name]              NVARCHAR (200) NOT NULL,
    [Slug]              NVARCHAR (200) NOT NULL,
    [ImageFileID]       INT            NULL,
    [SortOrder]         INT            CONSTRAINT [DF_ProductCategories_SortOrder] DEFAULT ((0)) NOT NULL,
    [IsActive]          BIT            CONSTRAINT [DF_ProductCategories_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_ProductCategories] PRIMARY KEY CLUSTERED ([ProductCategoryID] ASC),
    CONSTRAINT [FK_ProductCategories_FileRecord] FOREIGN KEY ([ImageFileID]) REFERENCES [dbo].[FileRecord] ([FileRecordID]),
    CONSTRAINT [FK_ProductCategories_ProductCategories] FOREIGN KEY ([ParentCategoryID]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryID]),
    CONSTRAINT [FK_ProductCategories_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

