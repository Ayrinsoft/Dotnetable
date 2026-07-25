CREATE TABLE [dbo].[WebsiteFeatures] (
    [WebsiteFeatureID] INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT           NOT NULL,
    [FeatureKey]       TINYINT       NOT NULL,
    [Enabled]          BIT NOT NULL,
    [CreatedAt]        DATETIME2 (0) NOT NULL,
    CONSTRAINT [PK_WebsiteFeatures] PRIMARY KEY CLUSTERED ([WebsiteFeatureID] ASC),
    CONSTRAINT [UQ_WebsiteFeatures_WebsiteID_FeatureKey] UNIQUE ([WebsiteID], [FeatureKey]),
    CONSTRAINT [FK_WebsiteFeatures_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

-- FeatureKey maps to Dotnetable.Domain.Enums.WebsiteFeatureKey. One row per module the site has
-- ever had an opinion on; absence of a row means "use the WebsiteType default", not "disabled".
