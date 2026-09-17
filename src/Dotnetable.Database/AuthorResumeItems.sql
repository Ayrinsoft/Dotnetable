CREATE TABLE [dbo].[AuthorResumeItems] (
    [AuthorResumeItemID] INT             IDENTITY (1, 1) NOT NULL,
    [AuthorProfileID]    INT             NOT NULL,
    [ItemType]           TINYINT         NOT NULL,
    [Title]              NVARCHAR (200)  NOT NULL,
    [Organization]       NVARCHAR (200)  NULL,
    [Location]           NVARCHAR (150)  NULL,
    [Description]        NVARCHAR (4000) NULL,
    [Url]                NVARCHAR (512)  NULL,
    [StartDate]          DATE            NULL,
    [EndDate]            DATE            NULL,
    [IsCurrent]          BIT             NOT NULL,
    [ShowInTimeline]     BIT             CONSTRAINT [DF_AuthorResumeItems_ShowInTimeline] DEFAULT ((1)) NOT NULL,
    [SortOrder]          INT             NOT NULL,
    [IsActive]           BIT             CONSTRAINT [DF_AuthorResumeItems_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_AuthorResumeItems] PRIMARY KEY CLUSTERED ([AuthorResumeItemID] ASC),
    CONSTRAINT [FK_AuthorResumeItems_AuthorProfiles] FOREIGN KEY ([AuthorProfileID]) REFERENCES [dbo].[AuthorProfiles] ([AuthorProfileID]) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_AuthorResumeItems_AuthorProfileID]
    ON [dbo].[AuthorResumeItems] ([AuthorProfileID] ASC);
