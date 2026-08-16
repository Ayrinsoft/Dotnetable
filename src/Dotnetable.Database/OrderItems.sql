CREATE TABLE [dbo].[OrderItems] (
    [OrderItemID]      INT             IDENTITY (1, 1) NOT NULL,
    [OrderID]          INT             NOT NULL,
    [WebsiteID]        INT             NOT NULL,
    [SourceWebsiteID]  INT             NOT NULL,
    [ProductVariantID] INT             NULL,
    [VendorProductID]  INT             NULL,
    [VendorID]         INT             NULL,
    [TitleSnapshot]    NVARCHAR (300)  NOT NULL,
    [SkuSnapshot]      NVARCHAR (100)  NOT NULL,
    [Quantity]         INT             NOT NULL,
    [UnitPrice]        DECIMAL (18, 4) NOT NULL,
    [UnitPriceUsd]     DECIMAL (18, 4) NOT NULL,
    [UnitCostUsd]      DECIMAL (18, 4) NOT NULL,
    [CatalogUnitPrice] DECIMAL (18, 4) NOT NULL,
    [UnitMarkup]       DECIMAL (18, 4) NOT NULL,
    [DiscountAmount]   DECIMAL (18, 4) NOT NULL,
    [TotalPrice]       DECIMAL (18, 4) NOT NULL,
    CONSTRAINT [PK_OrderItems] PRIMARY KEY CLUSTERED ([OrderItemID] ASC),
    CONSTRAINT [FK_OrderItems_Orders] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_OrderItems_ProductVariants] FOREIGN KEY ([ProductVariantID]) REFERENCES [dbo].[ProductVariants] ([ProductVariantID]),
    CONSTRAINT [FK_OrderItems_VendorProducts] FOREIGN KEY ([VendorProductID]) REFERENCES [dbo].[VendorProducts] ([VendorProductID]),
    CONSTRAINT [FK_OrderItems_Vendors] FOREIGN KEY ([VendorID]) REFERENCES [dbo].[Vendors] ([VendorID]),
    CONSTRAINT [FK_OrderItems_Website1] FOREIGN KEY ([SourceWebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_OrderItems_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);




GO

CREATE NONCLUSTERED INDEX [IX_OrderItems_OrderID]
    ON [dbo].[OrderItems] ([OrderID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_OrderItems_ProductVariantID]
    ON [dbo].[OrderItems] ([ProductVariantID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_OrderItems_SourceWebsiteID]
    ON [dbo].[OrderItems] ([SourceWebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_OrderItems_VendorID]
    ON [dbo].[OrderItems] ([VendorID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_OrderItems_VendorProductID]
    ON [dbo].[OrderItems] ([VendorProductID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_OrderItems_WebsiteID]
    ON [dbo].[OrderItems] ([WebsiteID] ASC);
