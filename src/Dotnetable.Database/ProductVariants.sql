CREATE TABLE [dbo].[ProductVariants] (
    [ProductVariantID]  INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]         INT             NOT NULL,
    [ProductID]         INT             NOT NULL,
    [Sku]               NVARCHAR (100)  NOT NULL,
    [Title]             NVARCHAR (200)  NOT NULL,
    [IsDefault]         BIT             CONSTRAINT [DF_ProductVariants_IsDefault] DEFAULT ((0)) NOT NULL,
    [ImageFileID]       INT             NULL,
    [ReferencePriceUsd] DECIMAL (18, 4) NOT NULL,
    [CompareAtPriceUsd] DECIMAL (18, 4) NULL,
    [OverridePrice]     DECIMAL (18, 4) NULL,
    [Weight]            DECIMAL (10, 3) NULL,
    [Barcode]           NVARCHAR (100)  NULL,
    [IsActive]          BIT             CONSTRAINT [DF_ProductVariants_IsActive] DEFAULT ((1)) NOT NULL,
    [CreatedAt]         DATETIME        CONSTRAINT [DF_ProductVariants_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_ProductVariants] PRIMARY KEY CLUSTERED ([ProductVariantID] ASC),
    CONSTRAINT [FK_ProductVariants_FileRecords] FOREIGN KEY ([ImageFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_ProductVariants_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID]),
    CONSTRAINT [FK_ProductVariants_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductVariants_ImageFileID]
    ON [dbo].[ProductVariants] ([ImageFileID] ASC);

CREATE NONCLUSTERED INDEX [IX_ProductVariants_ProductID]
    ON [dbo].[ProductVariants] ([ProductID] ASC);

CREATE NONCLUSTERED INDEX [IX_ProductVariants_WebsiteID]
    ON [dbo].[ProductVariants] ([WebsiteID] ASC);
