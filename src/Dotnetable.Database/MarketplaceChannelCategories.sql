CREATE TABLE [dbo].[MarketplaceChannelCategories] (
    [MarketplaceChannelCategoryID] INT            IDENTITY (1, 1) NOT NULL,
    [MarketplaceChannelID]         INT            NOT NULL,
    [ProductCategoryID]            INT            NOT NULL,
    [IsIncluded]                   BIT            CONSTRAINT [DF_MarketplaceChannelCategories_IsIncluded] DEFAULT ((1)) NOT NULL,
    [RemoteCategoryID]             NVARCHAR (100) NULL,
    [RemoteCategoryPath]           NVARCHAR (400) NULL,
    CONSTRAINT [PK_MarketplaceChannelCategories] PRIMARY KEY CLUSTERED ([MarketplaceChannelCategoryID] ASC),
    CONSTRAINT [FK_MarketplaceChannelCategories_MarketplaceChannels] FOREIGN KEY ([MarketplaceChannelID]) REFERENCES [dbo].[MarketplaceChannels] ([MarketplaceChannelID]) ON DELETE CASCADE,
    CONSTRAINT [FK_MarketplaceChannelCategories_ProductCategories] FOREIGN KEY ([ProductCategoryID]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryID]) ON DELETE CASCADE
);


-- Per-channel treatment of one product category: whether its products are offered to the engine, and
-- the category id/path that engine expects. Each engine keeps its own taxonomy, so this cannot live
-- on ProductCategories.
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_MarketplaceChannelCategories_Channel_Category]
    ON [dbo].[MarketplaceChannelCategories] ([MarketplaceChannelID] ASC, [ProductCategoryID] ASC);
