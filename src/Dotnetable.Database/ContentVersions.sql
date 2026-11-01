CREATE TABLE [dbo].[ContentVersions] (
    [ContentVersionID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT            NOT NULL,
    [Name]             VARCHAR (40)   NOT NULL,
    [SortOrder]        INT            CONSTRAINT [DF_ContentVersions_SortOrder] DEFAULT ((0)) NOT NULL,
    [IsLatest]         BIT            CONSTRAINT [DF_ContentVersions_IsLatest] DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [IsPublished]      BIT            CONSTRAINT [DF_ContentVersions_IsPublished] DEFAULT (CONVERT([bit],(0))) NOT NULL,
    CONSTRAINT [PK_ContentVersions] PRIMARY KEY CLUSTERED ([ContentVersionID] ASC),
    CONSTRAINT [FK_ContentVersions_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]) ON DELETE CASCADE,
    CONSTRAINT [UQ_ContentVersions_Website_Name] UNIQUE NONCLUSTERED ([WebsiteID] ASC, [Name] ASC)
);
GO

CREATE NONCLUSTERED INDEX [IX_ContentVersions_WebsiteID]
    ON [dbo].[ContentVersions] ([WebsiteID] ASC);
GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Content release label such as 1.2. Not an API version and not a product version. Used by opt-in versioned posts (help centers, policies, release notes).', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ContentVersions', @level2type = N'COLUMN', @level2name = N'Name';
GO
