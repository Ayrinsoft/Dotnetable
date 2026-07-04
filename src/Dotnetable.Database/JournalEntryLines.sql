CREATE TABLE [dbo].[JournalEntryLines] (
    [JournalEntryLineID] INT             IDENTITY (1, 1) NOT NULL,
    [JournalEntryID]     INT             NOT NULL,
    [ChartOfAccountID]   INT             NOT NULL,
    [Debit]              DECIMAL (18, 4) CONSTRAINT [DF_JournalEntryLines_Debit] DEFAULT ((0)) NOT NULL,
    [Credit]             DECIMAL (18, 4) CONSTRAINT [DF_JournalEntryLines_Credit] DEFAULT ((0)) NOT NULL,
    [Description]        NVARCHAR (300)  NULL,
    CONSTRAINT [PK_JournalEntryLines] PRIMARY KEY CLUSTERED ([JournalEntryLineID] ASC),
    CONSTRAINT [FK_JournalEntryLines_ChartOfAccounts] FOREIGN KEY ([ChartOfAccountID]) REFERENCES [dbo].[ChartOfAccounts] ([ChartOfAccountID]),
    CONSTRAINT [FK_JournalEntryLines_JournalEntries] FOREIGN KEY ([JournalEntryID]) REFERENCES [dbo].[JournalEntries] ([JournalEntrieID])
);

