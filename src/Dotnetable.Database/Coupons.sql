CREATE TABLE [dbo].[Coupons] (
    [CouponID]             INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]            INT             NOT NULL,
    [Code]                 VARCHAR (40)    NOT NULL,
    [DiscountType]         TINYINT         NOT NULL,
    [DiscountValue]        DECIMAL (18, 4) NOT NULL,
    [MinOrderAmountUsd]    DECIMAL (18, 4) CONSTRAINT [DF_Coupons_MinOrderAmountUsd] DEFAULT ((0)) NOT NULL,
    [MaxDiscountAmountUsd] DECIMAL (18, 4) NULL,
    [UsageLimitTotal]      INT             NULL,
    [UsageLimitPerClient]  INT             NULL,
    [TimesUsed]            INT             CONSTRAINT [DF_Coupons_TimesUsed] DEFAULT ((0)) NOT NULL,
    [StartsAt]             DATETIME2 (0)   NULL,
    [EndsAt]               DATETIME2 (0)   NULL,
    [IsActive]             BIT             CONSTRAINT [DF_Coupons_IsActive] DEFAULT ((1)) NOT NULL,
    [CreatedByMemberID]    INT             NULL,
    [CreatedAt]            DATETIME2 (0) NOT NULL,
    CONSTRAINT [PK_Coupons] PRIMARY KEY CLUSTERED ([CouponID] ASC),
    CONSTRAINT [UQ_Coupons_WebsiteID_Code] UNIQUE ([WebsiteID], [Code]),
    CONSTRAINT [FK_Coupons_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_Coupons_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

-- DiscountType: 1 = Percent (DiscountValue is a percentage, MaxDiscountAmountUsd caps it),
-- 2 = FixedAmountUsd. TimesUsed is a cached counter kept in sync with CouponRedemptions.
GO

CREATE NONCLUSTERED INDEX [IX_Coupons_CreatedByMemberID]
    ON [dbo].[Coupons] ([CreatedByMemberID] ASC);
