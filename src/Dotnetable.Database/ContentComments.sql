CREATE TABLE [dbo].[ContentComments] (
    [ContentCommentID]    INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]           INT             NOT NULL,
    [PostID]              INT             NULL,
    [PageID]              INT             NULL,
    [ParentCommentID]     INT             NULL,
    [WebsiteClientID]     INT             NULL,
    [AuthorMemberID]      INT             NULL,
    [AuthorName]          NVARCHAR (150)  NOT NULL,
    [AuthorEmail]         NVARCHAR (256)  NULL,
    [Body]                NVARCHAR (4000) NOT NULL,
    [Status]              TINYINT         NOT NULL,
    [IpAddress]           NVARCHAR (64)   NULL,
    [UserAgent]           NVARCHAR (512)  NULL,
    [CreatedAt]           DATETIME2 (0)   NOT NULL,
    [ModeratedAt]         DATETIME2 (0)   NULL,
    [ModeratedByMemberID] INT             NULL,
    CONSTRAINT [PK_ContentComments] PRIMARY KEY CLUSTERED ([ContentCommentID] ASC),
    CONSTRAINT [FK_ContentComments_ContentComments] FOREIGN KEY ([ParentCommentID]) REFERENCES [dbo].[ContentComments] ([ContentCommentID]),
    CONSTRAINT [FK_ContentComments_Members_Author] FOREIGN KEY ([AuthorMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_ContentComments_Members_Moderator] FOREIGN KEY ([ModeratedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_ContentComments_Pages] FOREIGN KEY ([PageID]) REFERENCES [dbo].[Pages] ([PageID]),
    CONSTRAINT [FK_ContentComments_Posts] FOREIGN KEY ([PostID]) REFERENCES [dbo].[Posts] ([PostID]),
    CONSTRAINT [FK_ContentComments_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID]),
    CONSTRAINT [FK_ContentComments_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ContentComments_AuthorMemberID]
    ON [dbo].[ContentComments] ([AuthorMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ContentComments_ModeratedByMemberID]
    ON [dbo].[ContentComments] ([ModeratedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ContentComments_PageID]
    ON [dbo].[ContentComments] ([PageID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ContentComments_ParentCommentID]
    ON [dbo].[ContentComments] ([ParentCommentID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ContentComments_PostID]
    ON [dbo].[ContentComments] ([PostID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ContentComments_WebsiteClientID]
    ON [dbo].[ContentComments] ([WebsiteClientID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ContentComments_WebsiteID_Status]
    ON [dbo].[ContentComments] ([WebsiteID] ASC, [Status] ASC);
