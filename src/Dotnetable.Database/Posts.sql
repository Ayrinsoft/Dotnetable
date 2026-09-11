CREATE TABLE [dbo].[Posts] (
    [PostID]              INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]           INT             NOT NULL,
    [PostTypeID]          INT             NOT NULL,
    [AuthorMemberID]      INT             NULL,
    [Slug]                NVARCHAR (300)  NOT NULL,
    [Title]               NVARCHAR (300)  NOT NULL,
    [Excerpt]             NVARCHAR (1000) NULL,
    [Content]             NVARCHAR (MAX)  NULL,
    [MetaTitle]           NVARCHAR (300)  NULL,
    [MetaDescription]     NVARCHAR (500)  NULL,
    [MetaKeywords]        NVARCHAR (500)  NULL,
    [FeaturedImageFileID] INT             NULL,
    [Status]              TINYINT NOT NULL,
    [PublishedAt]         DATETIME        NULL,
    [ScheduledAt]         DATETIME        NULL,
    [IsFeatured]          BIT NOT NULL,
    [ViewCount]           INT NOT NULL,
    [CommentsEnabled]     BIT NOT NULL,
    [IsActive]            BIT NOT NULL,
    [CreatedAt]           DATETIME NOT NULL,
    [UpdatedAt]           DATETIME NOT NULL,
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
