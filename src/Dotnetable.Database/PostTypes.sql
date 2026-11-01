CREATE TABLE [dbo].[PostTypes] (
    [PostTypeID]      INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT            NOT NULL,
    [Name]            NVARCHAR (100) NOT NULL,
    [Slug]            NVARCHAR (100) NOT NULL,
    [HasCategories]   BIT NOT NULL,
    [HasTags]         BIT NOT NULL,
    [HasAuthor]       BIT NOT NULL,
    [CommentsEnabled]      BIT NOT NULL,
    [SupportsStableKey]    BIT CONSTRAINT [DF_PostTypes_SupportsStableKey] DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [SupportsVersioning]   BIT CONSTRAINT [DF_PostTypes_SupportsVersioning] DEFAULT (CONVERT([bit],(0))) NOT NULL,
    CONSTRAINT [PK_PostTypes] PRIMARY KEY CLUSTERED ([PostTypeID] ASC),
    CONSTRAINT [FK_PostTypes_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_PostTypes_WebsiteID]
    ON [dbo].[PostTypes] ([WebsiteID] ASC);
GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Opt-in. Posts of this type may keep a stable key. Blog types leave this off.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PostTypes', @level2type = N'COLUMN', @level2name = N'SupportsStableKey';
GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Opt-in. Posts of this type may have release-scoped editions that share a stable key. Also allows a stable key.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'PostTypes', @level2type = N'COLUMN', @level2name = N'SupportsVersioning';
GO

