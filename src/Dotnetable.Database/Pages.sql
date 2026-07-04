CREATE TABLE [dbo].[Pages] (
    [PageID]            INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]         INT            NOT NULL,
    [ParentPageID]      INT            NULL,
    [Slug]              NVARCHAR (300) NOT NULL,
    [Title]             NVARCHAR (300) NOT NULL,
    [Content]           NVARCHAR (MAX) NULL,
    [Template]          NVARCHAR (100) NULL,
    [IsHomepage]        BIT            CONSTRAINT [DF_Pages_IsHomepage_1] DEFAULT ((0)) NOT NULL,
    [Status]            TINYINT        CONSTRAINT [DF_Pages_Status_1] DEFAULT ((1)) NOT NULL,
    [SortOrder]         INT            CONSTRAINT [DF_Pages_SortOrder] DEFAULT ((0)) NOT NULL,
    [IsActive]          BIT            CONSTRAINT [DF_Pages_IsActive_1] DEFAULT ((1)) NOT NULL,
    [CreatedByMemberID] INT            NULL,
    [CreatedAt]         DATETIME2 (0)  CONSTRAINT [DF_Pages_CreatedAt_1] DEFAULT (sysutcdatetime()) NOT NULL,
    [UpdatedAt]         DATETIME2 (0)  CONSTRAINT [DF_Pages_UpdatedAt_1] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_Pages] PRIMARY KEY CLUSTERED ([PageID] ASC),
    CONSTRAINT [FK_Pages_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_Pages_Pages] FOREIGN KEY ([ParentPageID]) REFERENCES [dbo].[Pages] ([PageID]),
    CONSTRAINT [FK_Pages_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

