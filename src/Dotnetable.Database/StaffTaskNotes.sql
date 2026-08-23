CREATE TABLE [dbo].[StaffTaskNotes] (
    [StaffTaskNoteID]   INT             IDENTITY (1, 1) NOT NULL,
    [StaffTaskID]       INT             NOT NULL,
    [CreatedByMemberID] INT             NOT NULL,
    [Body]              NVARCHAR (2000) NOT NULL,
    [CreatedAt]         DATETIME        NOT NULL,
    CONSTRAINT [PK_StaffTaskNotes] PRIMARY KEY CLUSTERED ([StaffTaskNoteID] ASC),
    CONSTRAINT [FK_StaffTaskNotes_StaffTasks] FOREIGN KEY ([StaffTaskID]) REFERENCES [dbo].[StaffTasks] ([StaffTaskID]) ON DELETE CASCADE,
    CONSTRAINT [FK_StaffTaskNotes_CreatedByMembers] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID])
);
GO

CREATE NONCLUSTERED INDEX [IX_StaffTaskNotes_StaffTaskID]
    ON [dbo].[StaffTaskNotes] ([StaffTaskID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_StaffTaskNotes_CreatedByMemberID]
    ON [dbo].[StaffTaskNotes] ([CreatedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_StaffTaskNotes_CreatedAt]
    ON [dbo].[StaffTaskNotes] ([CreatedAt] ASC);
GO
