CREATE TABLE [dbo].[OrderDigitalAssets] (
    [OrderDigitalAssetID] INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]           INT             NOT NULL,
    [WebsiteClientID]     INT             NOT NULL,
    [OrderID]             INT             NOT NULL,
    [OrderItemID]         INT             NOT NULL,
    [ProductID]           INT             NOT NULL,
    [ProductType]         TINYINT         NOT NULL,
    [TitleSnapshot]       NVARCHAR (400)  NOT NULL,
    [DigitalDownloadUrl]  NVARCHAR (1000) NULL,
    [DigitalServiceUrl]   NVARCHAR (1000) NULL,
    [DigitalDeliveryNote] NVARCHAR (2000) NULL,
    [IsActive]            BIT             NOT NULL,
    [GrantedAt]           DATETIME        NOT NULL,
    CONSTRAINT [PK_OrderDigitalAssets] PRIMARY KEY CLUSTERED ([OrderDigitalAssetID] ASC),
    CONSTRAINT [FK_OrderDigitalAssets_OrderItems] FOREIGN KEY ([OrderItemID]) REFERENCES [dbo].[OrderItems] ([OrderItemID]),
    CONSTRAINT [FK_OrderDigitalAssets_Orders] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_OrderDigitalAssets_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID]),
    CONSTRAINT [FK_OrderDigitalAssets_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID]),
    CONSTRAINT [FK_OrderDigitalAssets_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_OrderDigitalAssets_OrderItemID]
    ON [dbo].[OrderDigitalAssets] ([OrderItemID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_OrderDigitalAssets_WebsiteClientID]
    ON [dbo].[OrderDigitalAssets] ([WebsiteClientID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_OrderDigitalAssets_OrderID]
    ON [dbo].[OrderDigitalAssets] ([OrderID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_OrderDigitalAssets_WebsiteID]
    ON [dbo].[OrderDigitalAssets] ([WebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_OrderDigitalAssets_ProductID]
    ON [dbo].[OrderDigitalAssets] ([ProductID] ASC);
