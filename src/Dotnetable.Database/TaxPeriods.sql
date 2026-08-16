CREATE TABLE [dbo].[TaxPeriods] (
    [TaxPeriodID]           INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]             INT             NOT NULL,
    [PeriodCode]            NVARCHAR (30)   NOT NULL,
    [FromDate]              DATE            NOT NULL,
    [ToDate]                DATE            NOT NULL,
    [Status]                TINYINT         NOT NULL,
    [OutputTaxSnapshot]     DECIMAL (18, 4) NOT NULL,
    [SettlementTaxSnapshot] DECIMAL (18, 4) NOT NULL,
    [NetTaxSnapshot]        DECIMAL (18, 4) NOT NULL,
    [OutputOrderCount]      INT             NOT NULL,
    [SettlementCount]       INT             NOT NULL,
    [Note]                  NVARCHAR (1000) NULL,
    [CreatedByMemberID]     INT             NULL,
    [ClosedByMemberID]      INT             NULL,
    [ClosedAt]              DATETIME2 (7)   NULL,
    [SnapshotAt]            DATETIME2 (7)   NULL,
    [CreatedAt]             DATETIME2 (7)   NOT NULL,
    CONSTRAINT [PK_TaxPeriods] PRIMARY KEY CLUSTERED ([TaxPeriodID] ASC),
    CONSTRAINT [FK_TaxPeriods_Members_ClosedByMemberID] FOREIGN KEY ([ClosedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_TaxPeriods_Members_CreatedByMemberID] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_TaxPeriods_Websites_WebsiteID] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TaxPeriods_Website_Code]
    ON [dbo].[TaxPeriods] ([WebsiteID] ASC, [PeriodCode] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_TaxPeriods_Website_Status]
    ON [dbo].[TaxPeriods] ([WebsiteID] ASC, [Status] ASC, [FromDate] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_TaxPeriods_CreatedByMemberID]
    ON [dbo].[TaxPeriods]([CreatedByMemberID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TaxPeriods_ClosedByMemberID]
    ON [dbo].[TaxPeriods]([ClosedByMemberID] ASC);

