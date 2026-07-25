CREATE TABLE [dbo].[InventoryItems] (
    [InventoryItemID]  INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT             NOT NULL,
    [ProductVariantID] INT             NOT NULL,
    [QuantityOnHand]   INT NOT NULL,
    [QuantityReserved] INT NOT NULL,
    [ReorderLevel]     INT NOT NULL,
    [AvgCostUsd]       DECIMAL (18, 4) NOT NULL,
    [RowVersion]       ROWVERSION      NOT NULL,
    CONSTRAINT [PK_InventoryItems] PRIMARY KEY CLUSTERED ([InventoryItemID] ASC),
    CONSTRAINT [FK_InventoryItems_ProductVariants] FOREIGN KEY ([ProductVariantID]) REFERENCES [dbo].[ProductVariants] ([ProductVariantID]),
    CONSTRAINT [FK_InventoryItems_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_InventoryItems_ProductVariantID]
    ON [dbo].[InventoryItems] ([ProductVariantID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_InventoryItems_WebsiteID]
    ON [dbo].[InventoryItems] ([WebsiteID] ASC);
