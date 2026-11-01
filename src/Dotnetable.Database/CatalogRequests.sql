CREATE TABLE [dbo].[CatalogRequests] (
    [CatalogRequestID]     INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]            INT             NOT NULL,
    [RequestedByMemberID]  INT             NOT NULL,
    [Kind]                 TINYINT         NOT NULL,
    [Status]               TINYINT         DEFAULT (CONVERT([tinyint],(0))) NOT NULL,
    [ProposedCategoryName] NVARCHAR (200)  NULL,
    [ParentCategoryID]     INT             NULL,
    [ProductTitle]         NVARCHAR (300)  NOT NULL,
    [Details]              NVARCHAR (4000) NULL,
    [AdminNote]            NVARCHAR (2000) NULL,
    [CreatedAt]            DATETIME        NOT NULL,
    [ReviewedAt]           DATETIME        NULL,
    [ReviewedByMemberID]   INT             NULL,
    CONSTRAINT [PK_CatalogRequests] PRIMARY KEY CLUSTERED ([CatalogRequestID] ASC),
    CONSTRAINT [FK_CatalogRequests_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_CatalogRequests_Members] FOREIGN KEY ([RequestedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_CatalogRequests_ReviewedBy] FOREIGN KEY ([ReviewedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_CatalogRequests_ProductCategories] FOREIGN KEY ([ParentCategoryID]) REFERENCES [dbo].[ProductCategories] ([ProductCategoryID])
);
GO

CREATE NONCLUSTERED INDEX [IX_CatalogRequests_WebsiteID]
    ON [dbo].[CatalogRequests] ([WebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_CatalogRequests_RequestedByMemberID]
    ON [dbo].[CatalogRequests] ([RequestedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_CatalogRequests_ReviewedByMemberID]
    ON [dbo].[CatalogRequests] ([ReviewedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_CatalogRequests_ParentCategoryID]
    ON [dbo].[CatalogRequests] ([ParentCategoryID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_CatalogRequests_WebsiteID_Status]
    ON [dbo].[CatalogRequests] ([WebsiteID] ASC, [Status] ASC);
