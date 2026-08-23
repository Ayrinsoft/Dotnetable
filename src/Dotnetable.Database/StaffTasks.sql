CREATE TABLE [dbo].[StaffTasks] (
    [StaffTaskID]        INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]          INT            NOT NULL,
    [Title]              NVARCHAR (200) NOT NULL,
    [Description]        NVARCHAR (2000) NULL,
    [Status]             TINYINT        NOT NULL CONSTRAINT [DF_StaffTasks_Status] DEFAULT ((0)),
    [Priority]           TINYINT        NOT NULL CONSTRAINT [DF_StaffTasks_Priority] DEFAULT ((0)),
    [AssignedMemberID]   INT            NOT NULL,
    [CreatedByMemberID]  INT            NOT NULL,
    [DueAt]              DATETIME       NULL,
    [CompletedAt]        DATETIME       NULL,
    [RelatedKind]        TINYINT        NOT NULL CONSTRAINT [DF_StaffTasks_RelatedKind] DEFAULT ((0)),
    [RelatedEntityID]    INT            NULL,
    [RelatedLabel]       NVARCHAR (200) NULL,
    [CreatedAt]          DATETIME       NOT NULL,
    [UpdatedAt]          DATETIME       NOT NULL,
    CONSTRAINT [PK_StaffTasks] PRIMARY KEY CLUSTERED ([StaffTaskID] ASC),
    CONSTRAINT [FK_StaffTasks_AssignedMembers] FOREIGN KEY ([AssignedMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_StaffTasks_CreatedByMembers] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_StaffTasks_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_StaffTasks_WebsiteID]
    ON [dbo].[StaffTasks] ([WebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_StaffTasks_AssignedMemberID]
    ON [dbo].[StaffTasks] ([AssignedMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_StaffTasks_CreatedByMemberID]
    ON [dbo].[StaffTasks] ([CreatedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_StaffTasks_Website_Assigned_Status]
    ON [dbo].[StaffTasks] ([WebsiteID] ASC, [AssignedMemberID] ASC, [Status] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_StaffTasks_Related]
    ON [dbo].[StaffTasks] ([RelatedKind] ASC, [RelatedEntityID] ASC);
GO
