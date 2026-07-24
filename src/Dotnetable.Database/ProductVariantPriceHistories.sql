CREATE TABLE [dbo].[ProductVariantPriceHistories] (
    [ProductVariantPriceHistoryID] BIGINT          IDENTITY (1, 1) NOT NULL,
    [ProductVariantID]             INT             NOT NULL,
    [ReferencePriceUsd]            DECIMAL (18, 4) NOT NULL,
    [CompareAtPriceUsd]            DECIMAL (18, 4) NULL,
    [RecordedAt]                   DATETIME        CONSTRAINT [DF_ProductVariantPriceHistories_RecordedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    [ChangedByMemberId]            INT             NULL,
    CONSTRAINT [PK_ProductVariantPriceHistories] PRIMARY KEY CLUSTERED ([ProductVariantPriceHistoryID] ASC),
    CONSTRAINT [FK_ProductVariantPriceHistories_ProductVariants] FOREIGN KEY ([ProductVariantID]) REFERENCES [dbo].[ProductVariants] ([ProductVariantID]),
    CONSTRAINT [FK_ProductVariantPriceHistories_Members] FOREIGN KEY ([ChangedByMemberId]) REFERENCES [dbo].[Members] ([MemberID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductVariantPriceHistories_ProductVariantID_RecordedAt]
    ON [dbo].[ProductVariantPriceHistories] ([ProductVariantID] ASC, [RecordedAt] DESC);
