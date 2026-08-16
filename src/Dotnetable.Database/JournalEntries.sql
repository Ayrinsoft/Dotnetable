CREATE TABLE [dbo].[JournalEntries] (
    [JournalEntryID]         INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]              INT            NOT NULL,
    [EntryNumber]            NVARCHAR (30)  NOT NULL,
    [EntryDate]              DATE           NOT NULL,
    [Description]            NVARCHAR (500) NULL,
    [SourceType]             NVARCHAR (40)  NULL,
    [SourceId]               INT            NULL,
    [SourceKey]              NVARCHAR (80)  NULL,
    [CurrencyCode]           CHAR (3)       NOT NULL,
    [ReportToTax]            BIT            DEFAULT (CONVERT([bit],(1))) NOT NULL,
    [FiscalPeriodID]         INT            NULL,
    [IsPosted]               BIT            NOT NULL,
    [PostedAt]               DATETIME2 (7)  NULL,
    [PostedByMemberID]       INT            NULL,
    [IsReversed]             BIT            DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [ReversesJournalEntryID] INT            NULL,
    [CreatedByMemberID]      INT            NULL,
    [CreatedAt]              DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_JournalEntries] PRIMARY KEY CLUSTERED ([JournalEntryID] ASC),
    CONSTRAINT [FK_JournalEntries_CreatedBy] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_JournalEntries_Currencies] FOREIGN KEY ([CurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode]),
    CONSTRAINT [FK_JournalEntries_FiscalPeriods] FOREIGN KEY ([FiscalPeriodID]) REFERENCES [dbo].[FiscalPeriods] ([FiscalPeriodID]),
    CONSTRAINT [FK_JournalEntries_PostedBy] FOREIGN KEY ([PostedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_JournalEntries_Reverses] FOREIGN KEY ([ReversesJournalEntryID]) REFERENCES [dbo].[JournalEntries] ([JournalEntryID]),
    CONSTRAINT [FK_JournalEntries_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


GO

CREATE NONCLUSTERED INDEX [IX_JournalEntries_WebsiteID]
    ON [dbo].[JournalEntries] ([WebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_JournalEntries_EntryDate]
    ON [dbo].[JournalEntries] ([WebsiteID] ASC, [EntryDate] ASC, [IsPosted] ASC);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_JournalEntries_Website_SourceKey]
    ON [dbo].[JournalEntries] ([WebsiteID] ASC, [SourceKey] ASC)
    WHERE [SourceKey] IS NOT NULL;
GO

CREATE NONCLUSTERED INDEX [IX_JournalEntries_FiscalPeriodID]
    ON [dbo].[JournalEntries] ([FiscalPeriodID] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_JournalEntries_ReversesJournalEntryID]
    ON [dbo].[JournalEntries]([ReversesJournalEntryID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_JournalEntries_PostedByMemberID]
    ON [dbo].[JournalEntries]([PostedByMemberID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_JournalEntries_CurrencyCode]
    ON [dbo].[JournalEntries]([CurrencyCode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_JournalEntries_CreatedByMemberID]
    ON [dbo].[JournalEntries]([CreatedByMemberID] ASC);

