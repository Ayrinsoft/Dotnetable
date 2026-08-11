CREATE TABLE [dbo].[SupportInteractions] (
    [SupportInteractionID] INT             IDENTITY (1, 1) NOT NULL,
    [SupportSessionID]     INT             NOT NULL,
    [InteractionType]      TINYINT         NOT NULL,
    [Body]                 NVARCHAR (4000) NULL,
    [DurationSeconds]      INT             NULL,
    [CallOutcome]          TINYINT         NULL,
    [FromStatus]           TINYINT         NULL,
    [ToStatus]             TINYINT         NULL,
    [RelatedOrderID]       INT             NULL,
    [CreatedByMemberID]    INT             NULL,
    [IsInternal]           BIT             NOT NULL,
    [CreatedAt]            DATETIME        NOT NULL,
    CONSTRAINT [PK_SupportInteractions] PRIMARY KEY CLUSTERED ([SupportInteractionID] ASC),
    CONSTRAINT [FK_SupportInteractions_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_SupportInteractions_Orders] FOREIGN KEY ([RelatedOrderID]) REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_SupportInteractions_SupportSessions] FOREIGN KEY ([SupportSessionID]) REFERENCES [dbo].[SupportSessions] ([SupportSessionID])
);


GO

CREATE NONCLUSTERED INDEX [IX_SupportInteractions_SupportSessionID]
    ON [dbo].[SupportInteractions] ([SupportSessionID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SupportInteractions_CreatedByMemberID]
    ON [dbo].[SupportInteractions] ([CreatedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SupportInteractions_RelatedOrderID]
    ON [dbo].[SupportInteractions] ([RelatedOrderID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SupportInteractions_CreatedAt]
    ON [dbo].[SupportInteractions] ([CreatedAt] ASC);
GO
