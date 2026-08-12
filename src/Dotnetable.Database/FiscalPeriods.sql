CREATE TABLE [dbo].[FiscalPeriods] (
    [FiscalPeriodID]          INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]               INT            NOT NULL,
    [Name]                    NVARCHAR (100) NOT NULL,
    [PeriodFrom]              DATE           NOT NULL,
    [PeriodTo]                DATE           NOT NULL,
    [CloseDueDate]            DATE           NULL,
    [IsClosed]                BIT            DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [ClosedAt]                DATETIME2 (7)  NULL,
    [ClosedByMemberID]        INT            NULL,
    [OpeningJournalEntryID]   INT            NULL,
    [ClosingJournalEntryID]   INT            NULL,
    [CreatedAt]               DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_FiscalPeriods] PRIMARY KEY CLUSTERED ([FiscalPeriodID] ASC),
    CONSTRAINT [FK_FiscalPeriods_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_FiscalPeriods_Members] FOREIGN KEY ([ClosedByMemberID]) REFERENCES [dbo].[Members] ([MemberID])
);
GO

CREATE NONCLUSTERED INDEX [IX_FiscalPeriods_WebsiteID]
    ON [dbo].[FiscalPeriods] ([WebsiteID] ASC, [PeriodFrom] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_FiscalPeriods_Website_Closed]
    ON [dbo].[FiscalPeriods] ([WebsiteID] ASC, [IsClosed] ASC, [PeriodFrom] ASC);
GO
