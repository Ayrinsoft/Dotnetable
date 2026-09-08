CREATE TABLE [dbo].[MarketplaceChannels] (
    [MarketplaceChannelID] INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]            INT             NOT NULL,
    [Provider]             VARCHAR (50)    NOT NULL,
    [Title]                NVARCHAR (150)  NOT NULL,
    [SettingsJSON]         NVARCHAR (4000) NOT NULL,
    [FeedToken]            VARCHAR (64)    NOT NULL,
    [IncludeAllProducts]   BIT             CONSTRAINT [DF_MarketplaceChannels_IncludeAllProducts] DEFAULT ((1)) NOT NULL,
    [IsActive]             BIT             NOT NULL,
    [SortOrder]            INT             NOT NULL,
    [SyncIntervalMinutes]  INT             NOT NULL,
    [LastSyncAt]           DATETIME2 (0)   NULL,
    [LastSyncStatus]       TINYINT         NULL,
    [LastSyncMessage]      NVARCHAR (1000) NULL,
    [CreatedAt]            DATETIME2 (0)   NOT NULL,
    CONSTRAINT [PK_MarketplaceChannels] PRIMARY KEY CLUSTERED ([MarketplaceChannelID] ASC),
    CONSTRAINT [FK_MarketplaceChannels_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


-- One row per product search engine / marketplace a website has registered, mirroring
-- WebsiteSmsSettings. Provider matches IMarketplaceProvider.Key; SettingsJSON holds that provider's
-- own credentials and field mapping. FeedToken is the unguessable slug in the public feed URL.
GO

CREATE NONCLUSTERED INDEX [IX_MarketplaceChannels_WebsiteID]
    ON [dbo].[MarketplaceChannels] ([WebsiteID] ASC);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_MarketplaceChannels_FeedToken]
    ON [dbo].[MarketplaceChannels] ([FeedToken] ASC);
