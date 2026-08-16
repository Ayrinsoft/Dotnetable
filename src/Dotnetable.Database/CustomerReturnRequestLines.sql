CREATE TABLE [dbo].[CustomerReturnRequestLines] (
    [CustomerReturnRequestLineID] INT             IDENTITY (1, 1) NOT NULL,
    [CustomerReturnRequestID]     INT             NOT NULL,
    [OrderItemID]                 INT             NOT NULL,
    [ProductVariantID]            INT             NULL,
    [Quantity]                    INT             NOT NULL,
    [UnitPricePaid]               DECIMAL (18, 4) NOT NULL,
    [UnitRefundRequested]         DECIMAL (18, 4) NOT NULL,
    [UnitRefundApproved]          DECIMAL (18, 4) CONSTRAINT [DF_CustomerReturnRequestLines_UnitRefundApproved] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_CustomerReturnRequestLines] PRIMARY KEY CLUSTERED ([CustomerReturnRequestLineID] ASC),
    CONSTRAINT [FK_CustomerReturnRequestLines_Requests] FOREIGN KEY ([CustomerReturnRequestID]) REFERENCES [dbo].[CustomerReturnRequests] ([CustomerReturnRequestID]),
    CONSTRAINT [FK_CustomerReturnRequestLines_OrderItems] FOREIGN KEY ([OrderItemID]) REFERENCES [dbo].[OrderItems] ([OrderItemID])
);

GO
CREATE NONCLUSTERED INDEX [IX_CustomerReturnRequestLines_Request]
    ON [dbo].[CustomerReturnRequestLines] ([CustomerReturnRequestID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_CustomerReturnRequestLines_OrderItem]
    ON [dbo].[CustomerReturnRequestLines] ([OrderItemID] ASC);
