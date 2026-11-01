CREATE TABLE [dbo].[Products] (
    [ProductID]           INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]           INT             NOT NULL,
    [BrandID]             INT             NULL,
    [Slug]                NVARCHAR (300)  NOT NULL,
    [Title]               NVARCHAR (300)  NOT NULL,
    [ShortDescription]    NVARCHAR (1000) NULL,
    [Content]             NVARCHAR (MAX)  NULL,
    [ExpertReview]        NVARCHAR (MAX)  NULL,
    [FeaturedImageFileID] INT             NULL,
    [ProductType]         TINYINT         NOT NULL,
    [RequiresShipping]    BIT             NOT NULL,
    [DigitalDownloadUrl]  NVARCHAR (1000) NULL,
    [DigitalServiceUrl]   NVARCHAR (1000) NULL,
    [DigitalDeliveryNote] NVARCHAR (2000) NULL,
    [IsCatalogOnly]       BIT             NOT NULL,
    [IsPreOwned]          BIT             CONSTRAINT [DF_Products_IsPreOwned] DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [HasVariants]         BIT             NOT NULL,
    [AvgRating]           DECIMAL (3, 2)  NOT NULL,
    [RatingCount]         INT             NOT NULL,
    [FavoriteCount]       INT             CONSTRAINT [DF_Products_FavoriteCount] DEFAULT ((0)) NOT NULL,
    [Status]              TINYINT         NOT NULL,
    [SortOrder]           INT             NOT NULL,
    [IsActive]            BIT             NOT NULL,
    [CreatedByMemberID]   INT             NULL,
    [CreatedAt]           DATETIME        NOT NULL,
    [UpdatedAt]           DATETIME        NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([ProductID] ASC),
    CONSTRAINT [FK_Products_Brands] FOREIGN KEY ([BrandID]) REFERENCES [dbo].[Brands] ([BrandID]),
    CONSTRAINT [FK_Products_FileRecords] FOREIGN KEY ([FeaturedImageFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_Products_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_Products_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);




GO

CREATE NONCLUSTERED INDEX [IX_Products_BrandID]
    ON [dbo].[Products] ([BrandID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Products_CreatedByMemberId]
    ON [dbo].[Products] ([CreatedByMemberId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Products_FeaturedImageFileID]
    ON [dbo].[Products] ([FeaturedImageFileID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Products_WebsiteID]
    ON [dbo].[Products] ([WebsiteID] ASC);
GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Used item owned by one store, with that store''s photos and description. On a platform, only this kind of product may be created by a website other than website 1.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Products', @level2type = N'COLUMN', @level2name = N'IsPreOwned';
