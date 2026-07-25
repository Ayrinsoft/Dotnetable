CREATE TABLE [dbo].[JournalEntryLines] (
    [JournalEntryLineID] INT             IDENTITY (1, 1) NOT NULL,
    [JournalEntryID]     INT             NOT NULL,
    [ChartOfAccountID]   INT             NOT NULL,
    [Debit]              DECIMAL (18, 4) NOT NULL,
    [Credit]             DECIMAL (18, 4) NOT NULL,
    [Description]        NVARCHAR (300)  NULL,
    CONSTRAINT [PK_JournalEntryLines] PRIMARY KEY CLUSTERED ([JournalEntryLineID] ASC),
    CONSTRAINT [FK_JournalEntryLines_ChartOfAccounts] FOREIGN KEY ([ChartOfAccountID]) REFERENCES [dbo].[ChartOfAccounts] ([ChartOfAccountID]),
    CONSTRAINT [FK_JournalEntryLines_JournalEntries] FOREIGN KEY ([JournalEntryID]) REFERENCES [dbo].[JournalEntries] ([JournalEntryID])
);
GO

CREATE NONCLUSTERED INDEX [IX_JournalEntryLines_ChartOfAccountID]
    ON [dbo].[JournalEntryLines] ([ChartOfAccountID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_JournalEntryLines_JournalEntryID]
    ON [dbo].[JournalEntryLines] ([JournalEntryID] ASC);
