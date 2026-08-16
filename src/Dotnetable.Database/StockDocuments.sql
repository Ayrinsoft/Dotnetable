CREATE TABLE [dbo].[StockDocuments] (
    [StockDocumentID]     INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]           INT             NOT NULL,
    [DocumentNumber]      NVARCHAR (40)   NOT NULL,
    [DocumentType]        TINYINT         NOT NULL,
    [Status]              TINYINT         NOT NULL,
    [FromWarehouseID]     INT             NULL,
    [ToWarehouseID]       INT             NULL,
    [SupplierID]          INT             NULL,
    [OrderID]             INT             NULL,
    [PaymentRefundID]     INT             NULL,
    [Note]                NVARCHAR (1000) NULL,
    [RequestedByMemberID] INT             NULL,
    [ApprovedByMemberID]  INT             NULL,
    [PostedByMemberID]    INT             NULL,
    [SubmittedAt]         DATETIME2 (7)   NULL,
    [ApprovedAt]          DATETIME2 (7)   NULL,
    [PostedAt]            DATETIME2 (7)   NULL,
    [CreatedAt]           DATETIME2 (7)   NOT NULL,
    CONSTRAINT [PK_StockDocuments] PRIMARY KEY CLUSTERED ([StockDocumentID] ASC),
    CONSTRAINT [FK_StockDocuments_Members_ApprovedByMemberID] FOREIGN KEY ([ApprovedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_StockDocuments_Members_PostedByMemberID] FOREIGN KEY ([PostedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_StockDocuments_Members_RequestedByMemberID] FOREIGN KEY ([RequestedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_StockDocuments_Orders_OrderID] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_StockDocuments_PaymentRefunds_PaymentRefundID] FOREIGN KEY ([PaymentRefundID]) REFERENCES [dbo].[PaymentRefunds] ([PaymentRefundID]),
    CONSTRAINT [FK_StockDocuments_Suppliers_SupplierID] FOREIGN KEY ([SupplierID]) REFERENCES [dbo].[Suppliers] ([SupplierID]),
    CONSTRAINT [FK_StockDocuments_Warehouses_FromWarehouseID] FOREIGN KEY ([FromWarehouseID]) REFERENCES [dbo].[Warehouses] ([WarehouseID]),
    CONSTRAINT [FK_StockDocuments_Warehouses_ToWarehouseID] FOREIGN KEY ([ToWarehouseID]) REFERENCES [dbo].[Warehouses] ([WarehouseID]),
    CONSTRAINT [FK_StockDocuments_Websites_WebsiteID] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


GO

GO
CREATE NONCLUSTERED INDEX [IX_StockDocuments_PaymentRefundID] ON [dbo].[StockDocuments] ([PaymentRefundID] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_StockDocuments_WebsiteID]
    ON [dbo].[StockDocuments]([WebsiteID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_StockDocuments_ToWarehouseID]
    ON [dbo].[StockDocuments]([ToWarehouseID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_StockDocuments_SupplierID]
    ON [dbo].[StockDocuments]([SupplierID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_StockDocuments_RequestedByMemberID]
    ON [dbo].[StockDocuments]([RequestedByMemberID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_StockDocuments_PostedByMemberID]
    ON [dbo].[StockDocuments]([PostedByMemberID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_StockDocuments_OrderID]
    ON [dbo].[StockDocuments]([OrderID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_StockDocuments_FromWarehouseID]
    ON [dbo].[StockDocuments]([FromWarehouseID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_StockDocuments_ApprovedByMemberID]
    ON [dbo].[StockDocuments]([ApprovedByMemberID] ASC);

