CREATE TABLE [dbo].[LedgerAccountMaps] (
    [LedgerAccountMapID] INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]          INT           NOT NULL,
    [TransactionType]    NVARCHAR (64) NOT NULL,
    [Flow]               TINYINT       NULL,
    [DebitAccountID]     INT           NOT NULL,
    [CreditAccountID]    INT           NOT NULL,
    [IsActive]           BIT           DEFAULT (CONVERT([bit],(1))) NOT NULL,
    CONSTRAINT [PK_LedgerAccountMaps] PRIMARY KEY CLUSTERED ([LedgerAccountMapID] ASC),
    CONSTRAINT [FK_LedgerAccountMaps_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_LedgerAccountMaps_Debit] FOREIGN KEY ([DebitAccountID]) REFERENCES [dbo].[ChartOfAccounts] ([ChartOfAccountID]),
    CONSTRAINT [FK_LedgerAccountMaps_Credit] FOREIGN KEY ([CreditAccountID]) REFERENCES [dbo].[ChartOfAccounts] ([ChartOfAccountID])
);
GO

CREATE NONCLUSTERED INDEX [IX_LedgerAccountMaps_Website_Type]
    ON [dbo].[LedgerAccountMaps] ([WebsiteID] ASC, [TransactionType] ASC, [IsActive] ASC);
GO
