CREATE TABLE [dbo].[VatInvoices] (
    [VatInvoiceID]          INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]             INT             NOT NULL,
    [Direction]             TINYINT         NOT NULL,
    [InvoiceNumber]         NVARCHAR (40)   NOT NULL,
    [InvoiceDate]           DATETIME        NOT NULL,
    [OrderID]               INT             NULL,
    [CurrencyCode]          CHAR (3)        NOT NULL,
    [OurName]               NVARCHAR (200)  NULL,
    [OurNationalId]         NVARCHAR (32)   NULL,
    [OurEconomicCode]       NVARCHAR (50)   NULL,
    [OurVatNumber]          NVARCHAR (50)   NULL,
    [OurRegistrationNumber] NVARCHAR (50)   NULL,
    [OurPhone]              NVARCHAR (32)   NULL,
    [OurEmail]              NVARCHAR (120)  NULL,
    [OurAddress]            NVARCHAR (500)  NULL,
    [OtherName]             NVARCHAR (200)  NULL,
    [OtherNationalId]       NVARCHAR (32)   NULL,
    [OtherEconomicCode]     NVARCHAR (50)   NULL,
    [OtherPhone]            NVARCHAR (32)   NULL,
    [OtherEmail]            NVARCHAR (120)  NULL,
    [OtherAddress]          NVARCHAR (500)  NULL,
    [OtherPostalCode]       NVARCHAR (20)   NULL,
    [NetTotal]              DECIMAL (18, 4) NOT NULL,
    [DiscountTotal]         DECIMAL (18, 4) NOT NULL,
    [TaxTotal]              DECIMAL (18, 4) NOT NULL,
    [PayableTotal]          DECIMAL (18, 4) NOT NULL,
    [PricesIncludeTax]      BIT             NOT NULL,
    [Note]                  NVARCHAR (1000) NULL,
    [CreatedByMemberID]     INT             NULL,
    [CreatedAt]             DATETIME        NOT NULL,
    CONSTRAINT [PK_VatInvoices] PRIMARY KEY CLUSTERED ([VatInvoiceID] ASC),
    CONSTRAINT [FK_VatInvoices_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_VatInvoices_Orders] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID]),
    CONSTRAINT [FK_VatInvoices_Members] FOREIGN KEY ([CreatedByMemberID]) REFERENCES [dbo].[Members] ([MemberID])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_VatInvoices_Number]
    ON [dbo].[VatInvoices] ([WebsiteID] ASC, [Direction] ASC, [InvoiceNumber] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_VatInvoices_OrderID]
    ON [dbo].[VatInvoices] ([OrderID] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_VatInvoices_Website_Date]
    ON [dbo].[VatInvoices] ([WebsiteID] ASC, [InvoiceDate] ASC);
GO

CREATE TABLE [dbo].[VatInvoiceLines] (
    [VatInvoiceLineID] INT             IDENTITY (1, 1) NOT NULL,
    [VatInvoiceID]     INT             NOT NULL,
    [Description]      NVARCHAR (1000) NOT NULL,
    [Quantity]         DECIMAL (18, 4) NOT NULL,
    [Unit]             NVARCHAR (40)   NULL,
    [UnitPrice]        DECIMAL (18, 4) NOT NULL,
    [DiscountAmount]   DECIMAL (18, 4) NOT NULL,
    [NetAmount]        DECIMAL (18, 4) NOT NULL,
    [TaxAmount]        DECIMAL (18, 4) NOT NULL,
    [PayableAmount]    DECIMAL (18, 4) NOT NULL,
    CONSTRAINT [PK_VatInvoiceLines] PRIMARY KEY CLUSTERED ([VatInvoiceLineID] ASC),
    CONSTRAINT [FK_VatInvoiceLines_VatInvoices] FOREIGN KEY ([VatInvoiceID]) REFERENCES [dbo].[VatInvoices] ([VatInvoiceID]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_VatInvoiceLines_Invoice]
    ON [dbo].[VatInvoiceLines] ([VatInvoiceID] ASC);
GO
