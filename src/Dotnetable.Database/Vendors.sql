CREATE TABLE [dbo].[Vendors] (
    [VendorID]               INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]              INT             NOT NULL,
    [Name]                   NVARCHAR (200)  NOT NULL,
    [Slug]                   NVARCHAR (200)  NOT NULL,
    [LogoFileID]             INT             NULL,
    [Rating]                 DECIMAL (3, 2)  NOT NULL,
    [IsActive]               BIT             NOT NULL,
    [SettlementMode]         TINYINT         NOT NULL,
    [CreditDays]             INT             NULL,
    [CreditLimit]            DECIMAL (18, 4) NULL,
    [CreditLimitUsd]         DECIMAL (18, 4) NULL,
    [VendorType]             TINYINT         NOT NULL,
    [MemberID]               INT             NULL,
    [LinkedWebsiteID]        INT             NULL,
    [SettlementCurrencyCode] CHAR (3)        NULL,
    [AvailableCredit]        DECIMAL (18, 4) NOT NULL,
    [AvailableCreditUsd]     DECIMAL (18, 4) NOT NULL,
    CONSTRAINT [PK_Vendors] PRIMARY KEY CLUSTERED ([VendorID] ASC),
    CONSTRAINT [FK_Vendors_Currencies_Settlement] FOREIGN KEY ([SettlementCurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode]),
    CONSTRAINT [FK_Vendors_FileRecords] FOREIGN KEY ([LogoFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_Vendors_LinkedWebsites] FOREIGN KEY ([LinkedWebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_Vendors_Members] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[Members] ([MemberID]),
    CONSTRAINT [FK_Vendors_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);







GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 = Immediate: every purchase from this vendor is settled instantly like a normal cash purchase. 1 = Credit: purchases accrue as credit and are batched into a periodic Settlements record due on CreditDays', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vendors', @level2type = N'COLUMN', @level2name = N'SettlementMode';
GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'number of days after the settlement period ends before payment is due; only meaningful when SettlementMode = Credit', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vendors', @level2type = N'COLUMN', @level2name = N'CreditDays';
GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'maximum outstanding credit balance allowed for this vendor, in USD; only meaningful when SettlementMode = Credit', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vendors', @level2type = N'COLUMN', @level2name = N'CreditLimitUsd';
GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 = Display-only title, 1 = Member login (manage own catalog), 2 = Linked website (inter-site virtual credit)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Vendors', @level2type = N'COLUMN', @level2name = N'VendorType';
GO

CREATE NONCLUSTERED INDEX [IX_Vendors_LogoFileID]
    ON [dbo].[Vendors] ([LogoFileID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Vendors_WebsiteID]
    ON [dbo].[Vendors] ([WebsiteID] ASC);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Vendors_MemberID]
    ON [dbo].[Vendors] ([MemberID] ASC)
    WHERE [MemberID] IS NOT NULL;
GO

CREATE NONCLUSTERED INDEX [IX_Vendors_LinkedWebsiteID]
    ON [dbo].[Vendors] ([LinkedWebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Vendors_SettlementCurrencyCode]
    ON [dbo].[Vendors] ([SettlementCurrencyCode] ASC);
