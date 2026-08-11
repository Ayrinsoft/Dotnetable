CREATE TABLE [dbo].[ShippingMethods] (
    [ShippingMethodID]              INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]                     INT             NOT NULL,
    [Title]                         NVARCHAR (100)  NOT NULL,
    [CarrierName]                   NVARCHAR (100)  NULL,
    [IsActive]                      BIT             NOT NULL,
    [SortOrder]                     INT             NOT NULL,
    [CodMinPrice]                   DECIMAL (18, 4) DEFAULT ((0.0)) NOT NULL,
    [CodMinPriceUsd]                DECIMAL (18, 4) DEFAULT ((0.0)) NOT NULL,
    [LogoFileID]                    INT             NULL,
    [PrepaidMinPrice]               DECIMAL (18, 4) DEFAULT ((0.0)) NOT NULL,
    [PrepaidMinPriceUsd]            DECIMAL (18, 4) DEFAULT ((0.0)) NOT NULL,
    [SupportsCod]                   BIT             DEFAULT (CONVERT([bit],(1))) NOT NULL,
    [SupportsPrepaid]               BIT             DEFAULT (CONVERT([bit],(1))) NOT NULL,
    [FreeShippingMinOrderAmount]    DECIMAL (18, 4) DEFAULT ((0.0)) NOT NULL,
    [FreeShippingMinOrderAmountUsd] DECIMAL (18, 4) DEFAULT ((0.0)) NOT NULL,
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
