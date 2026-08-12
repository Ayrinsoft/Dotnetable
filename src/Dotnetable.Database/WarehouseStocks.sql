CREATE TABLE [dbo].[WarehouseStocks] (
    [WarehouseStockID]  INT           IDENTITY (1, 1) NOT NULL,
    [WarehouseID]       INT           NOT NULL,
    [ProductVariantID]  INT           NOT NULL,
    [QuantityOnHand]    INT           NOT NULL,
    [QuantityReserved]  INT           DEFAULT ((0)) NOT NULL,
    [RowVersion]        ROWVERSION    NOT NULL,
    CONSTRAINT [PK_WarehouseStocks] PRIMARY KEY CLUSTERED ([WarehouseStockID] ASC),
    CONSTRAINT [FK_WarehouseStocks_Warehouses] FOREIGN KEY ([WarehouseID]) REFERENCES [dbo].[Warehouses] ([WarehouseID]),
    CONSTRAINT [FK_WarehouseStocks_ProductVariants] FOREIGN KEY ([ProductVariantID]) REFERENCES [dbo].[ProductVariants] ([ProductVariantID])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_WarehouseStocks_Warehouse_Variant] ON [dbo].[WarehouseStocks] ([WarehouseID] ASC, [ProductVariantID] ASC);
GO
