CREATE TABLE [dbo].[MenuItems] (
    [MenuItemID]        INT            IDENTITY (1, 1) NOT NULL,
    [MenuID]            INT            NOT NULL,
    [ParentItemID]      INT            NULL,
    [ItemType]          TINYINT        NOT NULL,
    [PageID]            INT            NULL,
    [PostID]            INT            NULL,
    [CategoryID]        INT            NULL,
    [ProductID]         INT            NULL,
    [ProductCategoryID] INT            NULL,
    [BrandID]           INT            NULL,
    [VendorID]          INT            NULL,
    [Url]               NVARCHAR (500) NULL,
    [Title]             NVARCHAR (200) NOT NULL,
    [Icon]              NVARCHAR (100) NULL,
    [CssClass]          NVARCHAR (100) NULL,
    [OpenInNewTab]      BIT            CONSTRAINT [DF_MenuItems_OpenInNewTab] DEFAULT ((0)) NOT NULL,
    [SortOrder]         INT            CONSTRAINT [DF_MenuItems_SortOrder] DEFAULT ((0)) NOT NULL,
    [IsActive]          BIT            CONSTRAINT [DF_MenuItems_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_MenuItems] PRIMARY KEY CLUSTERED ([MenuItemID] ASC),
    CONSTRAINT [FK_MenuItems_Brands] FOREIGN KEY ([BrandID]) REFERENCES [dbo].[Brands] ([BrandID]),
    CONSTRAINT [FK_MenuItems_Categories] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Categories] ([CategoryID]),
    CONSTRAINT [FK_MenuItems_Menus] FOREIGN KEY ([MenuID]) REFERENCES [dbo].[Menus] ([MenuID]),
    CONSTRAINT [FK_MenuItems_MenuItems] FOREIGN KEY ([ParentItemID]) REFERENCES [dbo].[MenuItems] ([MenuItemID]),
    CONSTRAINT [FK_MenuItems_Pages] FOREIGN KEY ([PageID]) REFERENCES [dbo].[Pages] ([PageID]),
    CONSTRAINT [FK_MenuItems_Posts] FOREIGN KEY ([PostID]) REFERENCES [dbo].[Posts] ([PostID]),
    CONSTRAINT [FK_MenuItems_ProductCategories] FOREIGN KEY ([ProductCategoryID]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryID]),
    CONSTRAINT [FK_MenuItems_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID]),
    CONSTRAINT [FK_MenuItems_Vendors] FOREIGN KEY ([VendorID]) REFERENCES [dbo].[Vendors] ([VendorID])
);
GO

CREATE NONCLUSTERED INDEX [IX_MenuItems_BrandID]
    ON [dbo].[MenuItems] ([BrandID] ASC);

CREATE NONCLUSTERED INDEX [IX_MenuItems_CategoryID]
    ON [dbo].[MenuItems] ([CategoryID] ASC);

CREATE NONCLUSTERED INDEX [IX_MenuItems_MenuID]
    ON [dbo].[MenuItems] ([MenuID] ASC);

CREATE NONCLUSTERED INDEX [IX_MenuItems_PageID]
    ON [dbo].[MenuItems] ([PageID] ASC);

CREATE NONCLUSTERED INDEX [IX_MenuItems_ParentItemID]
    ON [dbo].[MenuItems] ([ParentItemID] ASC);

CREATE NONCLUSTERED INDEX [IX_MenuItems_PostID]
    ON [dbo].[MenuItems] ([PostID] ASC);

CREATE NONCLUSTERED INDEX [IX_MenuItems_ProductCategoryID]
    ON [dbo].[MenuItems] ([ProductCategoryID] ASC);

CREATE NONCLUSTERED INDEX [IX_MenuItems_ProductID]
    ON [dbo].[MenuItems] ([ProductID] ASC);

CREATE NONCLUSTERED INDEX [IX_MenuItems_VendorID]
    ON [dbo].[MenuItems] ([VendorID] ASC);
