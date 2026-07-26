CREATE TABLE [dbo].[SupportSessions] (
    [SupportSessionID]      INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]             INT            NOT NULL,
    [WebsiteClientID]       INT            NULL,
    [SessionNumber]         NVARCHAR (30)  NOT NULL,
    [Status]                TINYINT        NOT NULL,
    [Priority]              TINYINT        NOT NULL,
    [Channel]               TINYINT        NOT NULL,
    [Category]              TINYINT        NOT NULL,
    [Subject]               NVARCHAR (256) NULL,
    [Tags]                  NVARCHAR (256) NULL,
    [CellphoneSnapshot]     VARCHAR (16)   NULL,
    [CountryCodeSnapshot]   VARCHAR (3)    NULL,
    [EmailSnapshot]         VARCHAR (64)   NULL,
    [CustomerNameSnapshot]  NVARCHAR (128) NULL,
    [RelatedOrderID]        INT            NULL,
    [AssignedMemberID]      INT            NULL,
    [CreatedByMemberID]     INT            NULL,
    [FirstResponseAt]       DATETIME       NULL,
    [ResolvedAt]            DATETIME       NULL,
    [ClosedAt]              DATETIME       NULL,
    [FirstResponseDueAt]    DATETIME       NULL,
    [ResolveDueAt]          DATETIME       NULL,
    [CallbackAt]            DATETIME       NULL,
    [CallbackNote]          NVARCHAR (500) NULL,
    [CreatedAt]             DATETIME       NOT NULL,
    [UpdatedAt]             DATETIME       NOT NULL,
    [LastInteractionAt]     DATETIME       NULL,
    [SatisfactionRating]    TINYINT        NULL,
    [Archive]               BIT            NOT NULL CONSTRAINT [DF_SupportSessions_Archive] DEFAULT ((0)),
    CONSTRAINT [PK_SupportSessions] PRIMARY KEY CLUSTERED ([SupportSessionID] ASC),
    CONSTRAINT [FK_SupportSessions_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_SupportSessions_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID]),
    CONSTRAINT [FK_SupportSessions_Orders] FOREIGN KEY ([RelatedOrderID]) REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_SupportSessions_AssignedMembers] FOREIGN KEY ([AssignedMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_SupportSessions_CreatedByMembers] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID])
);
GO

CREATE NONCLUSTERED INDEX [IX_SupportSessions_WebsiteID]
    ON [dbo].[SupportSessions] ([WebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SupportSessions_WebsiteClientID]
    ON [dbo].[SupportSessions] ([WebsiteClientID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SupportSessions_Status]
    ON [dbo].[SupportSessions] ([Status] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SupportSessions_AssignedMemberID]
    ON [dbo].[SupportSessions] ([AssignedMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SupportSessions_CreatedByMemberID]
    ON [dbo].[SupportSessions] ([CreatedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SupportSessions_RelatedOrderID]
    ON [dbo].[SupportSessions] ([RelatedOrderID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SupportSessions_CreatedAt]
    ON [dbo].[SupportSessions] ([CreatedAt] ASC);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SupportSessions_Website_SessionNumber]
    ON [dbo].[SupportSessions] ([WebsiteID] ASC, [SessionNumber] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SupportSessions_CellphoneSnapshot]
    ON [dbo].[SupportSessions] ([CellphoneSnapshot] ASC);
GO
