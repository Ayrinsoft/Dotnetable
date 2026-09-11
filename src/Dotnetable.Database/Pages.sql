CREATE TABLE [dbo].[Pages] (
    [PageID]            INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]         INT            NOT NULL,
    [ParentPageID]      INT            NULL,
    [Slug]              NVARCHAR (300) NOT NULL,
    [Title]             NVARCHAR (300) NOT NULL,
    [Content]           NVARCHAR (MAX) NULL,
    [MetaTitle]         NVARCHAR (300) NULL,
    [MetaDescription]   NVARCHAR (500) NULL,
    [MetaKeywords]      NVARCHAR (500) NULL,
    [Template]          NVARCHAR (100) NULL,
    [IsHomepage]        BIT NOT NULL,
    [Status]            TINYINT NOT NULL,
    [SortOrder]         INT NOT NULL,
    [IsActive]          BIT NOT NULL,
    [CreatedByMemberID] INT            NULL,
    [CreatedAt]         DATETIME2 (0) NOT NULL,
    [UpdatedAt]         DATETIME2 (0) NOT NULL,
    CONSTRAINT [PK_Pages] PRIMARY KEY CLUSTERED ([PageID] ASC),
    CONSTRAINT [FK_Pages_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_Pages_Pages] FOREIGN KEY ([ParentPageID]) REFERENCES [dbo].[Pages] ([PageID]),
    CONSTRAINT [FK_Pages_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_Pages_CreatedByMemberID]
    ON [dbo].[Pages] ([CreatedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Pages_ParentPageID]
    ON [dbo].[Pages] ([ParentPageID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Pages_WebsiteID]
    ON [dbo].[Pages] ([WebsiteID] ASC);
