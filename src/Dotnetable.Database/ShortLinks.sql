CREATE TABLE [dbo].[ShortLinks] (
    [ShortLinkID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]   INT            NOT NULL,
    [Code]        NVARCHAR (16)  NOT NULL,
    [TargetUrl]   NVARCHAR (1000) NOT NULL,
    [CreatedAt]   DATETIME       NOT NULL,
    CONSTRAINT [PK_ShortLinks] PRIMARY KEY CLUSTERED ([ShortLinkID] ASC),
    CONSTRAINT [FK_ShortLinks_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ShortLinks_WebsiteID_Code]
    ON [dbo].[ShortLinks] ([WebsiteID] ASC, [Code] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ShortLinks_WebsiteID_TargetUrl]
    ON [dbo].[ShortLinks] ([WebsiteID] ASC, [TargetUrl] ASC);
