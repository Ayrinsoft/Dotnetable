CREATE TABLE [dbo].[CouponRedemptions] (
    [CouponRedemptionID] INT             IDENTITY (1, 1) NOT NULL,
    [CouponID]           INT             NOT NULL,
    [OrderID]            INT             NOT NULL,
    [WebsiteClientID]    INT             NOT NULL,
    [DiscountAmountUsd]  DECIMAL (18, 4) NOT NULL,
    [RedeemedAt]         DATETIME2 (0)   NOT NULL,
    [DiscountAmount]     DECIMAL (18, 4) DEFAULT ((0.0)) NOT NULL,
    CONSTRAINT [PK_CouponRedemptions] PRIMARY KEY CLUSTERED ([CouponRedemptionID] ASC),
    CONSTRAINT [FK_CouponRedemptions_Coupons] FOREIGN KEY ([CouponID]) REFERENCES [dbo].[Coupons] ([CouponID]),
    CONSTRAINT [FK_CouponRedemptions_Orders] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_CouponRedemptions_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID])
);



-- One redemption row per order (an order uses at most one coupon). Enforces UsageLimitPerClient
-- by counting rows here per (CouponID, WebsiteClientID).
GO

CREATE NONCLUSTERED INDEX [IX_CouponRedemptions_CouponID]
    ON [dbo].[CouponRedemptions] ([CouponID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_CouponRedemptions_WebsiteClientID]
    ON [dbo].[CouponRedemptions] ([WebsiteClientID] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_CouponRedemptions_OrderID]
    ON [dbo].[CouponRedemptions]([OrderID] ASC);

