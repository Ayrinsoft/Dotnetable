CREATE TABLE [dbo].[VendorProducts] (
    [VendorProductID]   INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]         INT             NOT NULL,
    [VendorID]          INT             NOT NULL,
    [ProductVariantID]  INT             NOT NULL,
    [ReferencePriceUsd] DECIMAL (18, 4) NOT NULL,
    [OverridePrice]     DECIMAL (18, 4) NULL,
    [StockQuantity]     INT NOT NULL,
    [DeliveryDays]      INT NOT NULL,
    [IsActive]          BIT NOT NULL,
    CONSTRAINT [PK_VendorProducts] PRIMARY KEY CLUSTERED ([VendorProductID] ASC),
    CONSTRAINT [FK_VendorProducts_ProductVariants] FOREIGN KEY ([ProductVariantID]) REFERENCES [dbo].[ProductVariants] ([ProductVariantID]),
    CONSTRAINT [FK_VendorProducts_Vendors] FOREIGN KEY ([VendorID]) REFERENCES [dbo].[Vendors] ([VendorID]),
    CONSTRAINT [FK_VendorProducts_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_VendorProducts_ProductVariantID]
    ON [dbo].[VendorProducts] ([ProductVariantID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_VendorProducts_VendorID]
    ON [dbo].[VendorProducts] ([VendorID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_VendorProducts_WebsiteID]
    ON [dbo].[VendorProducts] ([WebsiteID] ASC);
