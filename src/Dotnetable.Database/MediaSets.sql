CREATE TABLE [dbo].[MediaSets] (
    [MediaSetID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]  INT            NOT NULL,
    [Name]       NVARCHAR (200) NOT NULL,
    [IsShared]   BIT            CONSTRAINT [DF_MediaSets_IsShared_1] DEFAULT ((0)) NOT NULL,
    [CreatedAt]  DATETIME       CONSTRAINT [DF_MediaSets_CreatedAt_1] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_MediaSets] PRIMARY KEY CLUSTERED ([MediaSetID] ASC),
    CONSTRAINT [FK_MediaSets_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_MediaSets_WebsiteID]
    ON [dbo].[MediaSets] ([WebsiteID] ASC);
