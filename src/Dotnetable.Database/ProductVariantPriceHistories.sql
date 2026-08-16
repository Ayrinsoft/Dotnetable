CREATE TABLE [dbo].[ProductVariantPriceHistories] (
    [ProductVariantPriceHistoryID] BIGINT          IDENTITY (1, 1) NOT NULL,
    [ProductVariantID]             INT             NOT NULL,
    [ReferencePrice]               DECIMAL (18, 4) NOT NULL,
    [CompareAtPrice]               DECIMAL (18, 4) NULL,
    [ReferencePriceUsd]            DECIMAL (18, 4) NOT NULL,
    [CompareAtPriceUsd]            DECIMAL (18, 4) NULL,
    [RecordedAt]                   DATETIME        NOT NULL,
    [ChangedByMemberId]            INT             NULL,
    CONSTRAINT [PK_ProductVariantPriceHistories] PRIMARY KEY CLUSTERED ([ProductVariantPriceHistoryID] ASC),
    CONSTRAINT [FK_ProductVariantPriceHistories_Members] FOREIGN KEY ([ChangedByMemberId]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_ProductVariantPriceHistories_ProductVariants] FOREIGN KEY ([ProductVariantID]) REFERENCES [dbo].[ProductVariants] ([ProductVariantID])
);




GO

CREATE NONCLUSTERED INDEX [IX_ProductVariantPriceHistories_ProductVariantID_RecordedAt]
    ON [dbo].[ProductVariantPriceHistories] ([ProductVariantID] ASC, [RecordedAt] DESC);

GO
CREATE NONCLUSTERED INDEX [IX_ProductVariantPriceHistories_ChangedByMemberId]
    ON [dbo].[ProductVariantPriceHistories]([ChangedByMemberId] ASC);

