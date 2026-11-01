CREATE TABLE [dbo].[GoogleProductTaxonomyEntries] (
    [GoogleProductTaxonomyEntryID] INT            IDENTITY (1, 1) NOT NULL,
    [GoogleId]                     INT            NOT NULL,
    [ParentGoogleId]               INT            NULL,
    [Name]                         NVARCHAR (300) NOT NULL,
    [FullPath]                     NVARCHAR (1000) NOT NULL,
    [Depth]                        TINYINT        NOT NULL,
    [LastSeenUtc]                  DATETIME       NOT NULL,
    [AbsentFromLatest]             BIT            DEFAULT (CONVERT([bit],(0))) NOT NULL,
    CONSTRAINT [PK_GoogleProductTaxonomyEntries] PRIMARY KEY CLUSTERED ([GoogleProductTaxonomyEntryID] ASC)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_GoogleProductTaxonomyEntries_GoogleId]
    ON [dbo].[GoogleProductTaxonomyEntries] ([GoogleId] ASC);
