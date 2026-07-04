CREATE TABLE [dbo].[VendorProducts] (
    [VendorProductID]   INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]         INT             NOT NULL,
    [VendorID]          INT             NOT NULL,
    [ProductVariantID]  INT             NOT NULL,
    [ReferencePriceUsd] DECIMAL (18, 4) NOT NULL,
    [OverridePrice]     DECIMAL (18, 4) NULL,
    [StockQuantity]     INT             CONSTRAINT [DF_VendorProducts_StockQuantity] DEFAULT ((0)) NOT NULL,
    [DeliveryDays]      INT             CONSTRAINT [DF_VendorProducts_DeliveryDays] DEFAULT ((1)) NOT NULL,
    [IsActive]          BIT             CONSTRAINT [DF_VendorProducts_IsActive_1] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_VendorProducts] PRIMARY KEY CLUSTERED ([VendorProductID] ASC),
    CONSTRAINT [FK_VendorProducts_ProductVariants] FOREIGN KEY ([ProductVariantID]) REFERENCES [dbo].[ProductVariants] ([ProductVariantID]),
    CONSTRAINT [FK_VendorProducts_Vendors] FOREIGN KEY ([VendorID]) REFERENCES [dbo].[Vendors] ([VendorID]),
    CONSTRAINT [FK_VendorProducts_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

