CREATE TABLE [dbo].[Carts] (
    [CartID]          INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT           NOT NULL,
    [WebsiteClientID] INT           NULL,
    [SessionKey]      VARCHAR (64)  NULL,
    [CouponID]        INT           NULL,
    [CreatedAt]       DATETIME2 (0) NOT NULL,
    [UpdatedAt]       DATETIME2 (0) NOT NULL,
    CONSTRAINT [PK_Carts] PRIMARY KEY CLUSTERED ([CartID] ASC),
    CONSTRAINT [FK_Carts_Coupons] FOREIGN KEY ([CouponID]) REFERENCES [dbo].[Coupons] ([CouponID]),
    CONSTRAINT [FK_Carts_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_Carts_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID])
);

-- Guest cart: WebsiteClientID NULL + SessionKey set; merged into the client's own cart on login.
GO

CREATE NONCLUSTERED INDEX [IX_Carts_CouponID]
    ON [dbo].[Carts] ([CouponID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Carts_WebsiteClientID]
    ON [dbo].[Carts] ([WebsiteClientID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Carts_WebsiteID]
    ON [dbo].[Carts] ([WebsiteID] ASC);
