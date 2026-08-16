CREATE TABLE [dbo].[CustomerReturnRequests] (
    [CustomerReturnRequestID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]               INT            NOT NULL,
    [OrderID]                 INT            NOT NULL,
    [WebsiteClientID]         INT            NOT NULL,
    [StockDocumentID]         INT            NULL,
    [Status]                  TINYINT        NOT NULL,
    [Reason]                  TINYINT        NOT NULL,
    [ReasonNote]              NVARCHAR (200) NULL,
    [Description]             NVARCHAR (2000) NULL,
    [ShipMethod]              NVARCHAR (100) NULL,
    [TrackingCode]            NVARCHAR (100) NULL,
    [ShippingPayer]           TINYINT        CONSTRAINT [DF_CustomerReturnRequests_ShippingPayer] DEFAULT ((0)) NOT NULL,
    [CurrencyCode]            NVARCHAR (3)   NOT NULL,
    [RequestedRefundTotal]    DECIMAL (18, 4) NOT NULL,
    [ApprovedRefundTotal]     DECIMAL (18, 4) CONSTRAINT [DF_CustomerReturnRequests_ApprovedRefundTotal] DEFAULT ((0)) NOT NULL,
    [ReviewNote]              NVARCHAR (1000) NULL,
    [ReviewedByMemberID]      INT            NULL,
    [ReviewedAt]              DATETIME2 (7)  NULL,
    [ShippedAt]               DATETIME2 (7)  NULL,
    [CreatedAt]               DATETIME2 (7)  NOT NULL,
    [UpdatedAt]               DATETIME2 (7)  NULL,
    CONSTRAINT [PK_CustomerReturnRequests] PRIMARY KEY CLUSTERED ([CustomerReturnRequestID] ASC),
    CONSTRAINT [FK_CustomerReturnRequests_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_CustomerReturnRequests_Orders] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_CustomerReturnRequests_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID]),
    CONSTRAINT [FK_CustomerReturnRequests_StockDocuments] FOREIGN KEY ([StockDocumentID]) REFERENCES [dbo].[StockDocuments] ([StockDocumentID]),
    CONSTRAINT [FK_CustomerReturnRequests_Members] FOREIGN KEY ([ReviewedByMemberID]) REFERENCES [dbo].[Members] ([MemberID])
);

GO
CREATE NONCLUSTERED INDEX [IX_CustomerReturnRequests_Website_Status]
    ON [dbo].[CustomerReturnRequests] ([WebsiteID] ASC, [Status] ASC, [CreatedAt] DESC);

GO
CREATE NONCLUSTERED INDEX [IX_CustomerReturnRequests_Order]
    ON [dbo].[CustomerReturnRequests] ([OrderID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_CustomerReturnRequests_Client]
    ON [dbo].[CustomerReturnRequests] ([WebsiteClientID] ASC, [CreatedAt] DESC);
