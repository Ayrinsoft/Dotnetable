CREATE TABLE [dbo].[WebsiteVisibleProductCategories] (
    [WebsiteVisibleProductCategoryID] INT IDENTITY (1, 1) NOT NULL,
    [WebsiteID]                       INT NOT NULL,
    [ProductCategoryID]               INT NOT NULL,
    CONSTRAINT [PK_WebsiteVisibleProductCategories] PRIMARY KEY CLUSTERED ([WebsiteVisibleProductCategoryID] ASC),
    CONSTRAINT [FK_WebsiteVisibleProductCategories_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]) ON DELETE CASCADE,
    CONSTRAINT [FK_WebsiteVisibleProductCategories_ProductCategories] FOREIGN KEY ([ProductCategoryID]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryID]) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteVisibleProductCategories_ProductCategoryID]
    ON [dbo].[WebsiteVisibleProductCategories] ([ProductCategoryID] ASC);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_WebsiteVisibleProductCategories_Website_Category]
    ON [dbo].[WebsiteVisibleProductCategories] ([WebsiteID] ASC, [ProductCategoryID] ASC);
GO
