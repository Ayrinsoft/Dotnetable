CREATE TABLE [dbo].[JournalEntries] (
    [JournalEntryID]  INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT            NOT NULL,
    [EntryNumber]     NVARCHAR (30)  NOT NULL,
    [EntryDate]       DATE           NOT NULL,
    [Description]     NVARCHAR (500) NULL,
    [SourceType]      TINYINT        NULL,
    [SourceId]        INT            NULL,
    [IsPosted]        BIT            CONSTRAINT [DF_JournalEntries_IsPosted] DEFAULT ((0)) NOT NULL,
    [CreatedAt]       DATETIME       CONSTRAINT [DF_JournalEntries_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_JournalEntries] PRIMARY KEY CLUSTERED ([JournalEntryID] ASC),
    CONSTRAINT [FK_JournalEntries_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

