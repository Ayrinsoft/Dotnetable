CREATE TABLE [dbo].[WebsiteContactInfos] (
    [WebsiteContactInfoID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]            INT            NOT NULL,
    [ContactType]          TINYINT        NOT NULL DEFAULT 0,
    [GroupTitle]           NVARCHAR (64)  NULL,
    [Title]                NVARCHAR (64)  NOT NULL,
    [Value]                NVARCHAR (256) NOT NULL,
    [Icon]                 NVARCHAR (64)  NULL,
    [SortOrder]            INT            NOT NULL,
    CONSTRAINT [PK_WebsiteContactInfos] PRIMARY KEY CLUSTERED ([WebsiteContactInfoID] ASC),
    CONSTRAINT [FK_WebsiteContactInfos_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteContactInfos_WebsiteID]
    ON [dbo].[WebsiteContactInfos] ([WebsiteID] ASC);
