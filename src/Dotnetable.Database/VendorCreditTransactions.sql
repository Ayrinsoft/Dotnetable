CREATE TABLE [dbo].[VendorCreditTransactions] (
    [VendorCreditTransactionID] INT             IDENTITY (1, 1) NOT NULL,
    [VendorID]                  INT             NOT NULL,
    [WebsiteID]                 INT             NOT NULL,
    [Amount]                    DECIMAL (18, 4) NOT NULL CONSTRAINT [DF_VendorCreditTransactions_Amount] DEFAULT ((0)),
    [AmountUsd]                 DECIMAL (18, 4) NOT NULL,
    [BalanceAfter]              DECIMAL (18, 4) NOT NULL CONSTRAINT [DF_VendorCreditTransactions_BalanceAfter] DEFAULT ((0)),
    [BalanceAfterUsd]           DECIMAL (18, 4) NOT NULL,
    [SourceType]                TINYINT         NOT NULL,
    [SourceOrderItemID]         INT             NULL,
    [MirrorOrderID]             INT             NULL,
    [Note]                      NVARCHAR (500)  NULL,
    [CreatedByMemberID]         INT             NULL,
    [CreatedAt]                 DATETIME NOT NULL,
    CONSTRAINT [PK_VendorCreditTransactions] PRIMARY KEY CLUSTERED ([VendorCreditTransactionID] ASC),
    CONSTRAINT [FK_VendorCreditTransactions_Vendors] FOREIGN KEY ([VendorID]) REFERENCES [dbo].[Vendors] ([VendorID]),
    CONSTRAINT [FK_VendorCreditTransactions_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_VendorCreditTransactions_OrderItems] FOREIGN KEY ([SourceOrderItemID]) REFERENCES [dbo].[OrderItems] ([OrderItemID]),
    CONSTRAINT [FK_VendorCreditTransactions_Orders] FOREIGN KEY ([MirrorOrderID]) REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_VendorCreditTransactions_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID])
);
GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'1 = Grant, 2 = Sale, 3 = Adjustment, 4 = Refund', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'VendorCreditTransactions', @level2type = N'COLUMN', @level2name = N'SourceType';
GO

CREATE NONCLUSTERED INDEX [IX_VendorCreditTransactions_VendorID]
    ON [dbo].[VendorCreditTransactions] ([VendorID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_VendorCreditTransactions_WebsiteID]
    ON [dbo].[VendorCreditTransactions] ([WebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_VendorCreditTransactions_SourceOrderItemID]
    ON [dbo].[VendorCreditTransactions] ([SourceOrderItemID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_VendorCreditTransactions_MirrorOrderID]
    ON [dbo].[VendorCreditTransactions] ([MirrorOrderID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_VendorCreditTransactions_CreatedByMemberID]
    ON [dbo].[VendorCreditTransactions] ([CreatedByMemberID] ASC);
