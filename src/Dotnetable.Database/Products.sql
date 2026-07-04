CREATE TABLE [dbo].[Products] (
    [ProductID]           INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]           INT             NOT NULL,
    [BrandID]             INT             NULL,
    [Slug]                NVARCHAR (300)  NOT NULL,
    [Title]               NVARCHAR (300)  NOT NULL,
    [ShortDescription]    NVARCHAR (1000) NULL,
    [FeaturedImageFileID] INT             NULL,
    [IsCatalogOnly]       BIT             CONSTRAINT [DF_Products_IsCatalogOnly] DEFAULT ((0)) NOT NULL,
    [HasVariants]         BIT             CONSTRAINT [DF_Products_HasVariants_1] DEFAULT ((0)) NOT NULL,
    [AvgRating]           DECIMAL (3, 2)  CONSTRAINT [DF_Products_AvgRating_1] DEFAULT ((0)) NOT NULL,
    [RatingCount]         INT             CONSTRAINT [DF_Products_RatingCount_1] DEFAULT ((0)) NOT NULL,
    [Status]              TINYINT         CONSTRAINT [DF_Products_Status_1] DEFAULT ((1)) NOT NULL,
    [SortOrder]           INT             CONSTRAINT [DF_Products_SortOrder] DEFAULT ((0)) NOT NULL,
    [IsActive]            BIT             CONSTRAINT [DF_Products_IsActive_1] DEFAULT ((1)) NOT NULL,
    [CreatedByMemberId]   INT             NULL,
    [CreatedAt]           DATETIME        CONSTRAINT [DF_Products_CreatedAt_1] DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAt]           DATETIME        CONSTRAINT [DF_Products_UpdatedAt_1] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([ProductID] ASC),
    CONSTRAINT [FK_Products_Brands] FOREIGN KEY ([BrandID]) REFERENCES [dbo].[Brands] ([BrandID]),
    CONSTRAINT [FK_Products_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

