CREATE TABLE [dbo].[TaxPeriods] (
    [TaxPeriodID]            INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]              INT             NOT NULL,
    [PeriodCode]             NVARCHAR (30)   NOT NULL,
    [FromDate]               DATE            NOT NULL,
    [ToDate]                 DATE            NOT NULL,
    [Status]                 TINYINT         NOT NULL,
    [OutputTaxSnapshot]      DECIMAL (18, 4) NOT NULL CONSTRAINT [DF_TaxPeriods_OutputTax] DEFAULT ((0)),
    [SettlementTaxSnapshot]  DECIMAL (18, 4) NOT NULL CONSTRAINT [DF_TaxPeriods_SettlementTax] DEFAULT ((0)),
    [NetTaxSnapshot]         DECIMAL (18, 4) NOT NULL CONSTRAINT [DF_TaxPeriods_NetTax] DEFAULT ((0)),
    [OutputOrderCount]       INT             NOT NULL CONSTRAINT [DF_TaxPeriods_OrderCount] DEFAULT ((0)),
    [SettlementCount]        INT             NOT NULL CONSTRAINT [DF_TaxPeriods_SettlementCount] DEFAULT ((0)),
    [Note]                   NVARCHAR (1000) NULL,
    [CreatedByMemberID]      INT             NULL,
    [ClosedByMemberID]       INT             NULL,
    [ClosedAt]               DATETIME2 (7)   NULL,
    [SnapshotAt]             DATETIME2 (7)   NULL,
    [CreatedAt]              DATETIME2 (7)   NOT NULL,
    CONSTRAINT [PK_TaxPeriods] PRIMARY KEY CLUSTERED ([TaxPeriodID] ASC),
    CONSTRAINT [FK_TaxPeriods_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_TaxPeriods_CreatedBy] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_TaxPeriods_ClosedBy] FOREIGN KEY ([ClosedByMemberID]) REFERENCES [dbo].[Members] ([MemberID])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TaxPeriods_Website_Code]
    ON [dbo].[TaxPeriods] ([WebsiteID] ASC, [PeriodCode] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_TaxPeriods_Website_Status]
    ON [dbo].[TaxPeriods] ([WebsiteID] ASC, [Status] ASC, [FromDate] ASC);
GO
