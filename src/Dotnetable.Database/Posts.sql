CREATE TABLE [dbo].[Posts] (
    [PostID]              INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]           INT             NOT NULL,
    [PostTypeID]          INT             NOT NULL,
    [AuthorMemberID]      INT             NULL,
    [Slug]                NVARCHAR (300)  NOT NULL,
    [Title]               NVARCHAR (300)  NOT NULL,
    [Excerpt]             NVARCHAR (1000) NULL,
    [Content]             NVARCHAR (MAX)  NULL,
    [FeaturedImageFileID] INT             NULL,
    [Status]              TINYINT         CONSTRAINT [DF_Posts_Status_1] DEFAULT ((1)) NOT NULL,
    [PublishedAt]         DATETIME        NULL,
    [ScheduledAt]         DATETIME        NULL,
    [IsFeatured]          BIT             CONSTRAINT [DF_Posts_IsFeatured_1] DEFAULT ((0)) NOT NULL,
    [ViewCount]           INT             CONSTRAINT [DF_Posts_ViewCount_1] DEFAULT ((0)) NOT NULL,
    [CommentsEnabled]     BIT             CONSTRAINT [DF_Posts_CommentsEnabled] DEFAULT ((1)) NOT NULL,
    [IsActive]            BIT             CONSTRAINT [DF_Posts_IsActive_1] DEFAULT ((1)) NOT NULL,
    [CreatedAt]           DATETIME        CONSTRAINT [DF_Posts_CreatedAt_1] DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAt]           DATETIME        CONSTRAINT [DF_Posts_UpdatedAt_1] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_Posts] PRIMARY KEY CLUSTERED ([PostID] ASC),
    CONSTRAINT [FK_Posts_FileRecords] FOREIGN KEY ([FeaturedImageFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_Posts_Members] FOREIGN KEY ([AuthorMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_Posts_PostTypes] FOREIGN KEY ([PostTypeID]) REFERENCES [dbo].[PostTypes] ([PostTypeID]),
    CONSTRAINT [FK_Posts_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_Posts_AuthorMemberID]
    ON [dbo].[Posts] ([AuthorMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Posts_FeaturedImageFileID]
    ON [dbo].[Posts] ([FeaturedImageFileID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Posts_PostTypeID]
    ON [dbo].[Posts] ([PostTypeID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Posts_WebsiteID]
    ON [dbo].[Posts] ([WebsiteID] ASC);
