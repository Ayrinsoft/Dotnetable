CREATE TABLE [dbo].[Advertisements] (
    [AdvertisementID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT            NOT NULL,
    [Location]        TINYINT        NOT NULL,
    [Keyword]         NVARCHAR (200) NOT NULL,
    [Url]             NVARCHAR (500) NOT NULL,
    [OpenInNewTab]    BIT            NOT NULL,
    [SortOrder]       INT            NOT NULL,
    [IsActive]        BIT            NOT NULL,
    [CreatedAt]       DATETIME       NOT NULL,
    CONSTRAINT [PK_Advertisements] PRIMARY KEY CLUSTERED ([AdvertisementID] ASC),
    CONSTRAINT [FK_Advertisements_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_Advertisements_WebsiteID]
    ON [dbo].[Advertisements] ([WebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Advertisements_WebsiteID_Location]
    ON [dbo].[Advertisements] ([WebsiteID] ASC, [Location] ASC);
