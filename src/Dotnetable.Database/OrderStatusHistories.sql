CREATE TABLE [dbo].[OrderStatusHistories] (
    [OrderStatusHistoryID] INT            IDENTITY (1, 1) NOT NULL,
    [OrderID]              INT            NOT NULL,
    [FromStatus]           TINYINT        NULL,
    [ToStatus]             TINYINT        NOT NULL,
    [Note]                 NVARCHAR (500) NULL,
    [CreatedByMemberID]    INT            NULL,
    [CreatedAt]            DATETIME       CONSTRAINT [DF_OrderStatusHistories_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_OrderStatusHistories] PRIMARY KEY CLUSTERED ([OrderStatusHistoryID] ASC),
    CONSTRAINT [FK_OrderStatusHistories_Member] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Member] ([MemberID]),
    CONSTRAINT [FK_OrderStatusHistories_Orders] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID])
);

