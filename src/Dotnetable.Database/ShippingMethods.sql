CREATE TABLE [dbo].[ShippingMethods] (
    [ShippingMethodID]     INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]            INT             NOT NULL,
    [Title]                NVARCHAR (100)  NOT NULL,
    [CarrierName]          NVARCHAR (100)  NULL,
    [LogoFileID]           INT             NULL,
    [SupportsPrepaid]      BIT             NOT NULL,
    [SupportsCod]          BIT             NOT NULL,
    [PrepaidMinPrice]      DECIMAL (18, 4) NOT NULL,
    [PrepaidMinPriceUsd]   DECIMAL (18, 4) NOT NULL,
    [CodMinPrice]          DECIMAL (18, 4) NOT NULL,
    [CodMinPriceUsd]       DECIMAL (18, 4) NOT NULL,
    [IsActive]             BIT             NOT NULL,
    [SortOrder]            INT             NOT NULL,
    CONSTRAINT [PK_ShippingMethods] PRIMARY KEY CLUSTERED ([ShippingMethodID] ASC),
    CONSTRAINT [FK_ShippingMethods_FileRecords] FOREIGN KEY ([LogoFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_ShippingMethods_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ShippingMethods_LogoFileID]
    ON [dbo].[ShippingMethods] ([LogoFileID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ShippingMethods_WebsiteID]
    ON [dbo].[ShippingMethods] ([WebsiteID] ASC);
