CREATE TABLE [dbo].[FiscalPeriods] (
    [FiscalPeriodID]        INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]             INT            NOT NULL,
    [Name]                  NVARCHAR (100) NOT NULL,
    [PeriodFrom]            DATE           NOT NULL,
    [PeriodTo]              DATE           NOT NULL,
    [CloseDueDate]          DATE           NULL,
    [IsClosed]              BIT            DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [ClosedAt]              DATETIME2 (7)  NULL,
    [ClosedByMemberID]      INT            NULL,
    [OpeningJournalEntryID] INT            NULL,
    [ClosingJournalEntryID] INT            NULL,
    [CreatedAt]             DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_FiscalPeriods] PRIMARY KEY CLUSTERED ([FiscalPeriodID] ASC),
    CONSTRAINT [FK_FiscalPeriods_JournalEntries_ClosingJournalEntryID] FOREIGN KEY ([ClosingJournalEntryID]) REFERENCES [dbo].[JournalEntries] ([JournalEntryID]),
    CONSTRAINT [FK_FiscalPeriods_JournalEntries_OpeningJournalEntryID] FOREIGN KEY ([OpeningJournalEntryID]) REFERENCES [dbo].[JournalEntries] ([JournalEntryID]),
    CONSTRAINT [FK_FiscalPeriods_Members] FOREIGN KEY ([ClosedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_FiscalPeriods_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


GO

CREATE NONCLUSTERED INDEX [IX_FiscalPeriods_WebsiteID]
    ON [dbo].[FiscalPeriods] ([WebsiteID] ASC, [PeriodFrom] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_FiscalPeriods_Website_Closed]
    ON [dbo].[FiscalPeriods] ([WebsiteID] ASC, [IsClosed] ASC, [PeriodFrom] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_FiscalPeriods_OpeningJournalEntryID]
    ON [dbo].[FiscalPeriods]([OpeningJournalEntryID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FiscalPeriods_ClosingJournalEntryID]
    ON [dbo].[FiscalPeriods]([ClosingJournalEntryID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FiscalPeriods_ClosedByMemberID]
    ON [dbo].[FiscalPeriods]([ClosedByMemberID] ASC);

