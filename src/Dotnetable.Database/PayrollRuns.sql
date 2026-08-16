CREATE TABLE [dbo].[PayrollRuns] (
    [PayrollRunID]           INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]              INT             NOT NULL,
    [RunNumber]              NVARCHAR (40)   NOT NULL,
    [PeriodFrom]             DATE            NOT NULL,
    [PeriodTo]               DATE            NOT NULL,
    [Status]                 TINYINT         NOT NULL,
    [TotalGross]             DECIMAL (18, 4) NOT NULL,
    [TotalEmployeeInsurance] DECIMAL (18, 4) NOT NULL,
    [TotalEmployerInsurance] DECIMAL (18, 4) NOT NULL,
    [TotalIncomeTax]         DECIMAL (18, 4) NOT NULL,
    [TotalNet]               DECIMAL (18, 4) NOT NULL,
    [CurrencyCode]           CHAR (3)        NOT NULL,
    [Note]                   NVARCHAR (MAX)  NULL,
    [CreatedByMemberID]      INT             NULL,
    [ApprovedByMemberID]     INT             NULL,
    [ApprovedAt]             DATETIME2 (7)   NULL,
    [PaidAt]                 DATETIME2 (7)   NULL,
    [CreatedAt]              DATETIME2 (7)   NOT NULL,
    CONSTRAINT [PK_PayrollRuns] PRIMARY KEY CLUSTERED ([PayrollRunID] ASC),
    CONSTRAINT [FK_PayrollRuns_Currencies_CurrencyCode] FOREIGN KEY ([CurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode]),
    CONSTRAINT [FK_PayrollRuns_Members_ApprovedByMemberID] FOREIGN KEY ([ApprovedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_PayrollRuns_Members_CreatedByMemberID] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_PayrollRuns_Websites_WebsiteID] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


GO
CREATE NONCLUSTERED INDEX [IX_PayrollRuns_WebsiteID]
    ON [dbo].[PayrollRuns]([WebsiteID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PayrollRuns_CurrencyCode]
    ON [dbo].[PayrollRuns]([CurrencyCode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PayrollRuns_CreatedByMemberID]
    ON [dbo].[PayrollRuns]([CreatedByMemberID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PayrollRuns_ApprovedByMemberID]
    ON [dbo].[PayrollRuns]([ApprovedByMemberID] ASC);

