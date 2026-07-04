CREATE TABLE [dbo].[StockMovements] (
    [StockMovementID]   INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]         INT             NOT NULL,
    [ProductVariantID]  INT             NOT NULL,
    [Type]              TINYINT         NOT NULL,
    [Quantity]          INT             NOT NULL,
    [UnitCostUsd]       DECIMAL (18, 4) CONSTRAINT [DF_StockMovements_UnitCostUsd] DEFAULT ((0)) NOT NULL,
    [UnitSalePriceUsd]  DECIMAL (18, 4) NULL,
    [SupplierID]        INT             NULL,
    [OrderID]           INT             NULL,
    [OrderItemID]       INT             NULL,
    [Note]              NVARCHAR (500)  NULL,
    [CreatedByMemberID] INT             NULL,
    [CreatedAt]         DATETIME2 (0)   CONSTRAINT [DF_StockMovements_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_StockMovements] PRIMARY KEY CLUSTERED ([StockMovementID] ASC),
    CONSTRAINT [FK_StockMovements_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_StockMovements_OrderItems] FOREIGN KEY ([OrderItemID]) REFERENCES [dbo].[OrderItems] ([OrderItemID]),
    CONSTRAINT [FK_StockMovements_Orders] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_StockMovements_ProductVariants] FOREIGN KEY ([ProductVariantID]) REFERENCES [dbo].[ProductVariants] ([ProductVariantID]),
    CONSTRAINT [FK_StockMovements_Suppliers] FOREIGN KEY ([SupplierID]) REFERENCES [dbo].[Suppliers] ([SupplierID]),
    CONSTRAINT [FK_StockMovements_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

