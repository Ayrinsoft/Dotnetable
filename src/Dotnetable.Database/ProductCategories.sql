CREATE TABLE [dbo].[ProductCategories] (
    [ProductCategoryID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]         INT            NOT NULL,
    [ParentCategoryID]  INT            NULL,
    [Name]              NVARCHAR (200) NOT NULL,
    [Slug]              NVARCHAR (200) NOT NULL,
    [ImageFileID]       INT            NULL,
    [SortOrder]         INT NOT NULL,
    [IsActive]          BIT NOT NULL,
    CONSTRAINT [PK_ProductCategories] PRIMARY KEY CLUSTERED ([ProductCategoryID] ASC),
    CONSTRAINT [FK_ProductCategories_FileRecords] FOREIGN KEY ([ImageFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_ProductCategories_ProductCategories] FOREIGN KEY ([ParentCategoryID]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryID]),
    CONSTRAINT [FK_ProductCategories_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductCategories_ImageFileID]
    ON [dbo].[ProductCategories] ([ImageFileID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ProductCategories_ParentCategoryID]
    ON [dbo].[ProductCategories] ([ParentCategoryID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ProductCategories_WebsiteID]
    ON [dbo].[ProductCategories] ([WebsiteID] ASC);
