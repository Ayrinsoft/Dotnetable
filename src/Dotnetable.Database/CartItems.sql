CREATE TABLE [dbo].[CartItems] (
    [CartItemID]       INT           IDENTITY (1, 1) NOT NULL,
    [CartID]           INT           NOT NULL,
    [ProductVariantID] INT           NOT NULL,
    [VendorProductID]  INT           NULL,
    [Quantity]         INT           NOT NULL,
    [AddedAt]          DATETIME2 (0) NOT NULL,
    CONSTRAINT [PK_CartItems] PRIMARY KEY CLUSTERED ([CartItemID] ASC),
    CONSTRAINT [FK_CartItems_Carts] FOREIGN KEY ([CartID]) REFERENCES [dbo].[Carts] ([CartID]),
    CONSTRAINT [FK_CartItems_ProductVariants] FOREIGN KEY ([ProductVariantID]) REFERENCES [dbo].[ProductVariants] ([ProductVariantID]),
    CONSTRAINT [FK_CartItems_VendorProducts] FOREIGN KEY ([VendorProductID]) REFERENCES [dbo].[VendorProducts] ([VendorProductID])
);



-- No price columns: like everywhere pre-purchase, display price is always computed at runtime
-- from ReferencePriceUsd * CurrencyRates, never stored, until it's snapshotted onto OrderItems.
GO

CREATE NONCLUSTERED INDEX [IX_CartItems_ProductVariantID]
    ON [dbo].[CartItems] ([ProductVariantID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_CartItems_VendorProductID]
    ON [dbo].[CartItems] ([VendorProductID] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_CartItems_CartID_ProductVariantID_VendorProductID]
    ON [dbo].[CartItems]([CartID] ASC, [ProductVariantID] ASC, [VendorProductID] ASC) WHERE ([VendorProductID] IS NOT NULL);

