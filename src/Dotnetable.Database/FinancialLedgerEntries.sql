CREATE TABLE [dbo].[FinancialLedgerEntries] (
    [FinancialLedgerEntryID] BIGINT          IDENTITY (1, 1) NOT NULL,
    [WebsiteID]              INT             NOT NULL,
    [TransactionType]        NVARCHAR (64)   NOT NULL,
    [Flow]                   TINYINT         NOT NULL,
    [Amount]                 DECIMAL (18, 4) NOT NULL,
    [AmountUsd]              DECIMAL (18, 4) NOT NULL,
    [CurrencyCode]           CHAR (3)        NOT NULL,
    [OccurredDate]           DATE            NOT NULL,
    [OccurredTime]           TIME (7)        NOT NULL,
    [OccurredAtUtc]          DATETIME2 (7)   NOT NULL,
    [Title]                  NVARCHAR (300)  NOT NULL,
    [Description]            NVARCHAR (1000) NULL,
    [ReportToTax]            BIT             DEFAULT (CONVERT([bit],(1))) NOT NULL,
    [VendorVisible]          BIT             DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [VendorID]               INT             NULL,
    [OrderID]                INT             NULL,
    [OrderItemID]            INT             NULL,
    [PaymentID]              INT             NULL,
    [SettlementID]           INT             NULL,
    [WebsiteClientID]        INT             NULL,
    [EventGroupId]           UNIQUEIDENTIFIER NOT NULL,
    [Version]                INT             DEFAULT ((1)) NOT NULL,
    [SupersedesEntryID]      BIGINT          NULL,
    [IsCurrent]              BIT             DEFAULT (CONVERT([bit],(1))) NOT NULL,
    [ChangeNote]             NVARCHAR (500)  NULL,
    [CreatedByMemberID]      INT             NULL,
    [CreatedAt]              DATETIME2 (7)   NOT NULL,
    [MetaJson]               NVARCHAR (2000) NULL,
    CONSTRAINT [PK_FinancialLedgerEntries] PRIMARY KEY CLUSTERED ([FinancialLedgerEntryID] ASC),
    CONSTRAINT [FK_FinancialLedgerEntries_Currencies] FOREIGN KEY ([CurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode]),
    CONSTRAINT [FK_FinancialLedgerEntries_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_FinancialLedgerEntries_Orders] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_FinancialLedgerEntries_OrderItems] FOREIGN KEY ([OrderItemID]) REFERENCES [dbo].[OrderItems] ([OrderItemID]),
    CONSTRAINT [FK_FinancialLedgerEntries_Payments] FOREIGN KEY ([PaymentID]) REFERENCES [dbo].[Payments] ([PaymentID]),
    CONSTRAINT [FK_FinancialLedgerEntries_Settlements] FOREIGN KEY ([SettlementID]) REFERENCES [dbo].[Settlements] ([SettlementID]),
    CONSTRAINT [FK_FinancialLedgerEntries_Vendors] FOREIGN KEY ([VendorID]) REFERENCES [dbo].[Vendors] ([VendorID]),
    CONSTRAINT [FK_FinancialLedgerEntries_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID]),
    CONSTRAINT [FK_FinancialLedgerEntries_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_FinancialLedgerEntries_Supersedes] FOREIGN KEY ([SupersedesEntryID]) REFERENCES [dbo].[FinancialLedgerEntries] ([FinancialLedgerEntryID])
);
GO

CREATE NONCLUSTERED INDEX [IX_FinancialLedgerEntries_Website_Date]
    ON [dbo].[FinancialLedgerEntries] ([WebsiteID] ASC, [OccurredDate] ASC, [IsCurrent] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_FinancialLedgerEntries_OrderID]
    ON [dbo].[FinancialLedgerEntries] ([OrderID] ASC, [IsCurrent] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_FinancialLedgerEntries_VendorID]
    ON [dbo].[FinancialLedgerEntries] ([VendorID] ASC, [VendorVisible] ASC, [IsCurrent] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_FinancialLedgerEntries_Type]
    ON [dbo].[FinancialLedgerEntries] ([WebsiteID] ASC, [TransactionType] ASC, [IsCurrent] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_FinancialLedgerEntries_EventGroup]
    ON [dbo].[FinancialLedgerEntries] ([EventGroupId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_FinancialLedgerEntries_PaymentID]
    ON [dbo].[FinancialLedgerEntries] ([PaymentID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_FinancialLedgerEntries_SettlementID]
    ON [dbo].[FinancialLedgerEntries] ([SettlementID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_FinancialLedgerEntries_Supersedes]
    ON [dbo].[FinancialLedgerEntries] ([SupersedesEntryID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_FinancialLedgerEntries_CreatedByMemberID]
    ON [dbo].[FinancialLedgerEntries] ([CreatedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_FinancialLedgerEntries_CurrencyCode]
    ON [dbo].[FinancialLedgerEntries] ([CurrencyCode] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_FinancialLedgerEntries_WebsiteClientID]
    ON [dbo].[FinancialLedgerEntries] ([WebsiteClientID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_FinancialLedgerEntries_OrderItemID]
    ON [dbo].[FinancialLedgerEntries] ([OrderItemID] ASC);
GO
