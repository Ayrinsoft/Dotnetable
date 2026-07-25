CREATE TABLE [dbo].[InventoryItems] (
    [InventoryItemID]  INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT             NOT NULL,
    [ProductVariantID] INT             NOT NULL,
    [QuantityOnHand]   INT             CONSTRAINT [DF_InventoryItems_QuantityOnHand] DEFAULT ((0)) NOT NULL,
    [QuantityReserved] INT             CONSTRAINT [DF_InventoryItems_QuantityReserved] DEFAULT ((0)) NOT NULL,
    [ReorderLevel]     INT             CONSTRAINT [DF_InventoryItems_ReorderLevel] DEFAULT ((0)) NOT NULL,
    [AvgCostUsd]       DECIMAL (18, 4) CONSTRAINT [DF_InventoryItems_AvgCostUsd] DEFAULT ((0)) NOT NULL,
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
