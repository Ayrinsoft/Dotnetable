CREATE TABLE [dbo].[StockDocuments] (
    [StockDocumentID]      INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]            INT            NOT NULL,
    [DocumentNumber]       NVARCHAR (40)  NOT NULL,
    [DocumentType]         TINYINT        NOT NULL,
    [Status]               TINYINT        NOT NULL,
    [FromWarehouseID]      INT            NULL,
    [ToWarehouseID]        INT            NULL,
    [SupplierID]           INT            NULL,
    [OrderID]              INT            NULL,
    [Note]                 NVARCHAR (1000) NULL,
    [RequestedByMemberID]  INT            NULL,
    [ApprovedByMemberID]   INT            NULL,
    [PostedByMemberID]     INT            NULL,
    [SubmittedAt]          DATETIME2 (7)  NULL,
    [ApprovedAt]           DATETIME2 (7)  NULL,
    [PostedAt]             DATETIME2 (7)  NULL,
    [CreatedAt]            DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_StockDocuments] PRIMARY KEY CLUSTERED ([StockDocumentID] ASC),
    CONSTRAINT [FK_StockDocuments_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_StockDocuments_FromWarehouse] FOREIGN KEY ([FromWarehouseID]) REFERENCES [dbo].[Warehouses] ([WarehouseID]),
    CONSTRAINT [FK_StockDocuments_ToWarehouse] FOREIGN KEY ([ToWarehouseID]) REFERENCES [dbo].[Warehouses] ([WarehouseID]),
    CONSTRAINT [FK_StockDocuments_Suppliers] FOREIGN KEY ([SupplierID]) REFERENCES [dbo].[Suppliers] ([SupplierID]),
    CONSTRAINT [FK_StockDocuments_Orders] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_StockDocuments_RequestedBy] FOREIGN KEY ([RequestedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_StockDocuments_ApprovedBy] FOREIGN KEY ([ApprovedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_StockDocuments_PostedBy] FOREIGN KEY ([PostedByMemberID]) REFERENCES [dbo].[Members] ([MemberID])
);
GO
CREATE NONCLUSTERED INDEX [IX_StockDocuments_Website_Status] ON [dbo].[StockDocuments] ([WebsiteID] ASC, [Status] ASC, [DocumentType] ASC);
GO
