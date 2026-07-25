CREATE TABLE [dbo].[SettlementItems] (
    [SettlementItemID] INT             IDENTITY (1, 1) NOT NULL,
    [SettlementID]     INT             NOT NULL,
    [OrderItemID]      INT             NULL,
    [StockMovementID]  INT             NULL,
    [PaymentID]        INT             NULL,
    [Amount]           DECIMAL (18, 4) NOT NULL,
    [Description]      NVARCHAR (300)  NULL,
    CONSTRAINT [PK_SettlementItems] PRIMARY KEY CLUSTERED ([SettlementItemID] ASC),
    CONSTRAINT [FK_SettlementItems_OrderItems] FOREIGN KEY ([OrderItemID]) REFERENCES [dbo].[OrderItems] ([OrderItemID]),
    CONSTRAINT [FK_SettlementItems_Payments] FOREIGN KEY ([PaymentID]) REFERENCES [dbo].[Payments] ([PaymentID]),
    CONSTRAINT [FK_SettlementItems_Settlements] FOREIGN KEY ([SettlementID]) REFERENCES [dbo].[Settlements] ([SettlementID]),
    CONSTRAINT [FK_SettlementItems_StockMovements] FOREIGN KEY ([StockMovementID]) REFERENCES [dbo].[StockMovements] ([StockMovementID])
);
GO

CREATE NONCLUSTERED INDEX [IX_SettlementItems_OrderItemID]
    ON [dbo].[SettlementItems] ([OrderItemID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SettlementItems_PaymentID]
    ON [dbo].[SettlementItems] ([PaymentID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SettlementItems_SettlementID]
    ON [dbo].[SettlementItems] ([SettlementID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SettlementItems_StockMovementID]
    ON [dbo].[SettlementItems] ([StockMovementID] ASC);
