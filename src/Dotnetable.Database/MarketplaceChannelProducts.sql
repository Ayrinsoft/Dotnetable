CREATE TABLE [dbo].[MarketplaceChannelProducts] (
    [MarketplaceChannelProductID] INT            IDENTITY (1, 1) NOT NULL,
    [MarketplaceChannelID]        INT            NOT NULL,
    [ProductID]                   INT            NOT NULL,
    [IsIncluded]                  BIT            NOT NULL,
    [RemoteProductID]             NVARCHAR (100) NULL,
    [LastSyncedAt]                DATETIME2 (0)  NULL,
    CONSTRAINT [PK_MarketplaceChannelProducts] PRIMARY KEY CLUSTERED ([MarketplaceChannelProductID] ASC),
    CONSTRAINT [FK_MarketplaceChannelProducts_MarketplaceChannels] FOREIGN KEY ([MarketplaceChannelID]) REFERENCES [dbo].[MarketplaceChannels] ([MarketplaceChannelID]) ON DELETE CASCADE,
    CONSTRAINT [FK_MarketplaceChannelProducts_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID]) ON DELETE CASCADE
);


-- Per-product override on top of the channel's category rules: an explicit include for a product
-- whose category is not offered, or an exclude for one that otherwise would be. RemoteProductID is
-- filled in after a successful API push.
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_MarketplaceChannelProducts_Channel_Product]
    ON [dbo].[MarketplaceChannelProducts] ([MarketplaceChannelID] ASC, [ProductID] ASC);
