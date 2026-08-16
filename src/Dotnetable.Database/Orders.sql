CREATE TABLE [dbo].[Orders] (
    [OrderID]                INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]              INT             NOT NULL,
    [OrderNumber]            NVARCHAR (30)   NOT NULL,
    [WebsiteClientID]        INT             NOT NULL,
    [Status]                 TINYINT         NOT NULL,
    [CurrencyCode]           CHAR (3)        NOT NULL,
    [ExchangeRateToUsd]      DECIMAL (18, 6) NOT NULL,
    [SubTotal]               DECIMAL (18, 4) NOT NULL,
    [DiscountTotal]          DECIMAL (18, 4) NOT NULL,
    [ShippingTotal]          DECIMAL (18, 4) NOT NULL,
    [TaxTotal]               DECIMAL (18, 4) NOT NULL,
    [PricesIncludeTax]       BIT             NOT NULL,
    [TaxBreakdownJson]       NVARCHAR (4000) NULL,
    [GrandTotal]             DECIMAL (18, 4) NOT NULL,
    [GrandTotalUsd]          DECIMAL (18, 4) NOT NULL,
    [WebsiteClientAddressID] INT             NULL,
    [AddressSnapshot]        NVARCHAR (1000) NULL,
    [CouponID]               INT             NULL,
    [ShippingMethodID]       INT             NULL,
    [PreparationStatus]      TINYINT         DEFAULT (CONVERT([tinyint],(0))) NOT NULL,
    [ShippingStatus]         TINYINT         DEFAULT (CONVERT([tinyint],(0))) NOT NULL,
    [ShippingTrackingCode]   NVARCHAR (100)  NULL,
    [ShippedAt]              DATETIME        NULL,
    [Note]                   NVARCHAR (1000) NULL,
    [SalesChannel]           TINYINT         DEFAULT (CONVERT([tinyint],(1))) NOT NULL,
    [ReportToTax]            BIT             DEFAULT (CONVERT([bit],(1))) NOT NULL,
    [MarkupTotal]            DECIMAL (18, 4) NOT NULL,
    [CreatedByMemberID]      INT             NULL,
    [CreatedAt]              DATETIME        NOT NULL,
    [PaidAt]                 DATETIME        NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([OrderID] ASC),
    CONSTRAINT [FK_Orders_Coupons] FOREIGN KEY ([CouponID]) REFERENCES [dbo].[Coupons] ([CouponID]),
    CONSTRAINT [FK_Orders_Currencies] FOREIGN KEY ([CurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode]),
    CONSTRAINT [FK_Orders_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_Orders_ShippingMethods] FOREIGN KEY ([ShippingMethodID]) REFERENCES [dbo].[ShippingMethods] ([ShippingMethodID]),
    CONSTRAINT [FK_Orders_WebsiteClientAddresses] FOREIGN KEY ([WebsiteClientAddressID]) REFERENCES [dbo].[WebsiteClientAddresses] ([WebsiteClientAddressID]),
    CONSTRAINT [FK_Orders_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID]),
    CONSTRAINT [FK_Orders_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);




GO

CREATE NONCLUSTERED INDEX [IX_Orders_CouponID]
    ON [dbo].[Orders] ([CouponID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Orders_CreatedByMemberID]
    ON [dbo].[Orders] ([CreatedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Orders_CurrencyCode]
    ON [dbo].[Orders] ([CurrencyCode] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Orders_ShippingMethodID]
    ON [dbo].[Orders] ([ShippingMethodID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Orders_WebsiteClientAddressID]
    ON [dbo].[Orders] ([WebsiteClientAddressID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Orders_WebsiteClientID]
    ON [dbo].[Orders] ([WebsiteClientID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Orders_WebsiteID]
    ON [dbo].[Orders] ([WebsiteID] ASC);
