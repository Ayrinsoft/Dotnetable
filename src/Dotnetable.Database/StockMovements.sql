CREATE TABLE [dbo].[StockMovements] (
    [StockMovementID]   INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]         INT             NOT NULL,
    [ProductVariantID]  INT             NOT NULL,
    [Type]              TINYINT         NOT NULL,
    [Quantity]          INT             NOT NULL,
    [UnitCost]          DECIMAL (18, 4) NOT NULL CONSTRAINT [DF_StockMovements_UnitCost] DEFAULT ((0)),
    [UnitCostUsd]       DECIMAL (18, 4) NOT NULL,
    [UnitSalePrice]     DECIMAL (18, 4) NULL,
    [UnitSalePriceUsd]  DECIMAL (18, 4) NULL,
    [CurrencyCode]      CHAR (3)        NOT NULL,
    [ExchangeRateToUsd] DECIMAL (18, 6) NOT NULL,
    [SupplierID]        INT             NULL,
    [OrderID]           INT             NULL,
    [OrderItemID]       INT             NULL,
    [Note]              NVARCHAR (500)  NULL,
    [CreatedByMemberID] INT             NULL,
    [CreatedAt]         DATETIME2 (0) NOT NULL,
    CONSTRAINT [PK_StockMovements] PRIMARY KEY CLUSTERED ([StockMovementID] ASC),
    CONSTRAINT [FK_StockMovements_Currencies] FOREIGN KEY ([CurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode]),
    CONSTRAINT [FK_StockMovements_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_StockMovements_OrderItems] FOREIGN KEY ([OrderItemID]) REFERENCES [dbo].[OrderItems] ([OrderItemID]),
    CONSTRAINT [FK_StockMovements_Orders] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_StockMovements_ProductVariants] FOREIGN KEY ([ProductVariantID]) REFERENCES [dbo].[ProductVariants] ([ProductVariantID]),
    CONSTRAINT [FK_StockMovements_Suppliers] FOREIGN KEY ([SupplierID]) REFERENCES [dbo].[Suppliers] ([SupplierID]),
    CONSTRAINT [FK_StockMovements_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);



GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'website''s local currency rate snapshotted at the moment of this stock entry/exit, so accounting and reports can be reconstructed in local currency at that point in time', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'StockMovements', @level2type = N'COLUMN', @level2name = N'ExchangeRateToUsd';
GO

CREATE NONCLUSTERED INDEX [IX_StockMovements_CreatedByMemberID]
    ON [dbo].[StockMovements] ([CreatedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_StockMovements_CurrencyCode]
    ON [dbo].[StockMovements] ([CurrencyCode] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_StockMovements_OrderID]
    ON [dbo].[StockMovements] ([OrderID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_StockMovements_OrderItemID]
    ON [dbo].[StockMovements] ([OrderItemID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_StockMovements_ProductVariantID]
    ON [dbo].[StockMovements] ([ProductVariantID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_StockMovements_SupplierID]
    ON [dbo].[StockMovements] ([SupplierID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_StockMovements_WebsiteID]
    ON [dbo].[StockMovements] ([WebsiteID] ASC);
