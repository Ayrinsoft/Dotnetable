CREATE TABLE [dbo].[AuthorProfiles] (
    [AuthorProfileID] INT             IDENTITY (1, 1) NOT NULL,
    [MemberID]        INT             NOT NULL,
    [WebsiteID]       INT             NOT NULL,
    [Slug]            NVARCHAR (100)  NOT NULL,
    [DisplayName]     NVARCHAR (150)  NULL,
    [Headline]        NVARCHAR (200)  NULL,
    [Bio]             NVARCHAR (1000) NULL,
    [ShowBioOnPosts]  BIT             CONSTRAINT [DF_AuthorProfiles_ShowBioOnPosts] DEFAULT ((1)) NOT NULL,
    [ResumeEnabled]   BIT             NOT NULL,
    [About]           NVARCHAR (MAX)  NULL,
    [Location]        NVARCHAR (150)  NULL,
    [PublicEmail]     NVARCHAR (256)  NULL,
    [WebsiteUrl]      NVARCHAR (512)  NULL,
    [SocialLinksJson] NVARCHAR (4000) NULL,
    [Skills]          NVARCHAR (1000) NULL,
    [PhotoFileID]     INT             NULL,
    [ResumeFileID]    INT             NULL,
    [CreatedAt]       DATETIME2 (0)   NOT NULL,
    [UpdatedAt]       DATETIME2 (0)   NOT NULL,
    CONSTRAINT [PK_AuthorProfiles] PRIMARY KEY CLUSTERED ([AuthorProfileID] ASC),
    CONSTRAINT [FK_AuthorProfiles_Members] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[Members] ([MemberID]) ON DELETE CASCADE,
    CONSTRAINT [FK_AuthorProfiles_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_AuthorProfiles_FileRecords_Photo] FOREIGN KEY ([PhotoFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_AuthorProfiles_FileRecords_Resume] FOREIGN KEY ([ResumeFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_AuthorProfiles_MemberID]
    ON [dbo].[AuthorProfiles] ([MemberID] ASC);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_AuthorProfiles_WebsiteID_Slug]
    ON [dbo].[AuthorProfiles] ([WebsiteID] ASC, [Slug] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_AuthorProfiles_PhotoFileID]
    ON [dbo].[AuthorProfiles] ([PhotoFileID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_AuthorProfiles_ResumeFileID]
    ON [dbo].[AuthorProfiles] ([ResumeFileID] ASC);
