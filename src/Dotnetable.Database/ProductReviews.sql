CREATE TABLE [dbo].[ProductReviews] (
    [ProductReviewID]    INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]          INT             NOT NULL,
    [ProductID]          INT             NOT NULL,
    [ProductVariantID]   INT             NULL,
    [WebsiteClientID]    INT             NOT NULL,
    [Rating]             TINYINT         NOT NULL,
    [Title]              NVARCHAR (200)  NULL,
    [Body]               NVARCHAR (4000) NOT NULL,
    [ProsJson]           NVARCHAR (2000) NULL,
    [ConsJson]           NVARCHAR (2000) NULL,
    [IsVerifiedPurchase] BIT             CONSTRAINT [DF_ProductReviews_IsVerifiedPurchase] DEFAULT ((0)) NOT NULL,
    [Status]             TINYINT         CONSTRAINT [DF_ProductReviews_Status] DEFAULT ((1)) NOT NULL,
    [LikeCount]          INT             CONSTRAINT [DF_ProductReviews_LikeCount] DEFAULT ((0)) NOT NULL,
    [DislikeCount]       INT             CONSTRAINT [DF_ProductReviews_DislikeCount] DEFAULT ((0)) NOT NULL,
    [CreatedAt]          DATETIME2 (0)   CONSTRAINT [DF_ProductReviews_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    [Approved]           BIT             NOT NULL,
    CONSTRAINT [PK_ProductReviews] PRIMARY KEY CLUSTERED ([ProductReviewID] ASC),
    CONSTRAINT [FK_ProductReviews_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID]),
    CONSTRAINT [FK_ProductReviews_ProductVariants] FOREIGN KEY ([ProductVariantID]) REFERENCES [dbo].[ProductVariants] ([ProductVariantID]),
    CONSTRAINT [FK_ProductReviews_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID]),
    CONSTRAINT [FK_ProductReviews_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

