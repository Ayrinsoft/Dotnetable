CREATE TABLE [dbo].[Wishlists] (
    [WishlistID]      INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT           NOT NULL,
    [WebsiteClientID] INT           NOT NULL,
    [CreatedAt]       DATETIME2 (0) NOT NULL,
    CONSTRAINT [PK_Wishlists] PRIMARY KEY CLUSTERED ([WishlistID] ASC),
    CONSTRAINT [UQ_Wishlists_WebsiteClientID] UNIQUE ([WebsiteClientID]),
    CONSTRAINT [FK_Wishlists_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_Wishlists_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID])
);
GO

CREATE NONCLUSTERED INDEX [IX_Wishlists_WebsiteID]
    ON [dbo].[Wishlists] ([WebsiteID] ASC);
