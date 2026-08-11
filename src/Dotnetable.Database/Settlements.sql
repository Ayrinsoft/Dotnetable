CREATE TABLE [dbo].[Settlements] (
    [SettlementID]       INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]          INT             NOT NULL,
    [TargetType]         TINYINT         NOT NULL,
    [VendorID]           INT             NULL,
    [TargetWebsiteID]    INT             NULL,
    [SupplierID]         INT             NULL,
    [PeriodFrom]         DATE            NOT NULL,
    [PeriodTo]           DATE            NOT NULL,
    [TotalAmount]        DECIMAL (18, 4) NOT NULL,
    [CurrencyCode]       CHAR (3)        NOT NULL,
    [Status]             TINYINT         NOT NULL,
    [BankAccountID]      INT             NULL,
    [PaymentRefNumber]   NVARCHAR (100)  NULL,
    [Note]               NVARCHAR (500)  NULL,
    [CreatedByMemberID]  INT             NULL,
    [ApprovedByMemberID] INT             NULL,
    [PaidAt]             DATETIME        NULL,
    [CreatedAt]          DATETIME        NOT NULL,
    [NetAmount]          DECIMAL (18, 4) DEFAULT ((0.0)) NOT NULL,
    [TaxAmount]          DECIMAL (18, 4) DEFAULT ((0.0)) NOT NULL,
    [TaxRateSnapshot]    DECIMAL (9, 6)  NULL,
    CONSTRAINT [PK_Settlements] PRIMARY KEY CLUSTERED ([SettlementID] ASC),
    CONSTRAINT [FK_Settlements_BankAccounts] FOREIGN KEY ([BankAccountID]) REFERENCES [dbo].[BankAccounts] ([BankAccountID]),
    CONSTRAINT [FK_Settlements_Currencies] FOREIGN KEY ([CurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode]),
    CONSTRAINT [FK_Settlements_Member1] FOREIGN KEY ([ApprovedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_Settlements_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_Settlements_Suppliers] FOREIGN KEY ([SupplierID]) REFERENCES [dbo].[Suppliers] ([SupplierID]),
    CONSTRAINT [FK_Settlements_Vendors] FOREIGN KEY ([VendorID]) REFERENCES [dbo].[Vendors] ([VendorID]),
    CONSTRAINT [FK_Settlements_Website1] FOREIGN KEY ([TargetWebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_Settlements_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


GO

CREATE NONCLUSTERED INDEX [IX_Settlements_ApprovedByMemberID]
    ON [dbo].[Settlements] ([ApprovedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Settlements_BankAccountID]
    ON [dbo].[Settlements] ([BankAccountID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Settlements_CreatedByMemberID]
    ON [dbo].[Settlements] ([CreatedByMemberID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Settlements_CurrencyCode]
    ON [dbo].[Settlements] ([CurrencyCode] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Settlements_SupplierID]
    ON [dbo].[Settlements] ([SupplierID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Settlements_TargetWebsiteID]
    ON [dbo].[Settlements] ([TargetWebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Settlements_VendorID]
    ON [dbo].[Settlements] ([VendorID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Settlements_WebsiteID]
    ON [dbo].[Settlements] ([WebsiteID] ASC);
