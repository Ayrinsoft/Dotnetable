CREATE TABLE [dbo].[PriceLists] (
    [PriceListID]        INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]          INT             NOT NULL,
    [Title]              NVARCHAR (200)  NOT NULL,
    [Slug]               NVARCHAR (200)  NOT NULL,
    [Description]        NVARCHAR (2000) NULL,
    [Notes]              NVARCHAR (2000) NULL,
    [Source]             TINYINT         NOT NULL CONSTRAINT [DF_PriceLists_Source] DEFAULT ((0)),
    [Pricing]            TINYINT         NOT NULL CONSTRAINT [DF_PriceLists_Pricing] DEFAULT ((0)),
    [ProductCategoryID]  INT             NULL,
    [BrandID]            INT             NULL,
    [SortOrder]          INT             NOT NULL CONSTRAINT [DF_PriceLists_SortOrder] DEFAULT ((0)),
    [IsActive]           BIT             NOT NULL CONSTRAINT [DF_PriceLists_IsActive] DEFAULT ((1)),
    [CreatedAt]          DATETIME        NOT NULL,
    [UpdatedAt]          DATETIME        NOT NULL,
    CONSTRAINT [PK_PriceLists] PRIMARY KEY CLUSTERED ([PriceListID] ASC),
    CONSTRAINT [FK_PriceLists_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_PriceLists_ProductCategories] FOREIGN KEY ([ProductCategoryID]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryID]),
    CONSTRAINT [FK_PriceLists_Brands] FOREIGN KEY ([BrandID]) REFERENCES [dbo].[Brands] ([BrandID])
);
GO

CREATE NONCLUSTERED INDEX [IX_PriceLists_WebsiteID]
    ON [dbo].[PriceLists] ([WebsiteID] ASC);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_PriceLists_WebsiteID_Slug]
    ON [dbo].[PriceLists] ([WebsiteID] ASC, [Slug] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_PriceLists_ProductCategoryID]
    ON [dbo].[PriceLists] ([ProductCategoryID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_PriceLists_BrandID]
    ON [dbo].[PriceLists] ([BrandID] ASC);
GO
