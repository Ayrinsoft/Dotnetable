CREATE TABLE [dbo].[OrderStatusHistories] (
    [OrderStatusHistoryID] INT            IDENTITY (1, 1) NOT NULL,
    [OrderID]              INT            NOT NULL,
    [FromStatus]           TINYINT        NULL,
    [ToStatus]             TINYINT        NOT NULL,
    [Note]                 NVARCHAR (500) NULL,
    [CreatedByMemberID]    INT            NULL,
    [CreatedAt]            DATETIME NOT NULL,
    CONSTRAINT [PK_OrderStatusHistories] PRIMARY KEY CLUSTERED ([OrderStatusHistoryID] ASC),
    CONSTRAINT [FK_OrderStatusHistories_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_OrderStatusHistories_Orders] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID])
);
GO

CREATE NONCLUSTERED INDEX [IX_OrderStatusHistories_CreatedByMemberID]
    ON [dbo].[OrderStatusHistories] ([CreatedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_OrderStatusHistories_OrderID]
    ON [dbo].[OrderStatusHistories] ([OrderID] ASC);
