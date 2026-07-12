CREATE TABLE [dbo].[WishlistItems] (
    [WishlistItemID]   INT           IDENTITY (1, 1) NOT NULL,
    [WishlistID]       INT           NOT NULL,
    [ProductVariantID] INT           NOT NULL,
    [AddedAt]          DATETIME2 (0) CONSTRAINT [DF_WishlistItems_AddedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_WishlistItems] PRIMARY KEY CLUSTERED ([WishlistItemID] ASC),
    CONSTRAINT [UQ_WishlistItems_WishlistID_ProductVariantID] UNIQUE ([WishlistID], [ProductVariantID]),
    CONSTRAINT [FK_WishlistItems_ProductVariants] FOREIGN KEY ([ProductVariantID]) REFERENCES [dbo].[ProductVariants] ([ProductVariantID]),
    CONSTRAINT [FK_WishlistItems_Wishlists] FOREIGN KEY ([WishlistID]) REFERENCES [dbo].[Wishlists] ([WishlistID])
);
GO

CREATE NONCLUSTERED INDEX [IX_WishlistItems_ProductVariantID]
    ON [dbo].[WishlistItems] ([ProductVariantID] ASC);
