CREATE TABLE [dbo].[PaymentGateways] (
    [PaymentGatewayID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT            NOT NULL,
    [Name]             NVARCHAR (150) NOT NULL,
    [Provider]         NVARCHAR (50)  NOT NULL,
    [MerchantID]       NVARCHAR (200) NULL,
    [ApiKey]           NVARCHAR (500) NULL,
    [ApiSecret]        NVARCHAR (500) NULL,
    [IsSandbox]        BIT            CONSTRAINT [DF_PaymentGateways_IsSandbox] DEFAULT ((0)) NOT NULL,
    [SortOrder]        INT            CONSTRAINT [DF_PaymentGateways_SortOrder] DEFAULT ((0)) NOT NULL,
    [IsActive]         BIT            CONSTRAINT [DF_PaymentGateways_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_PaymentGateways] PRIMARY KEY CLUSTERED ([PaymentGatewayID] ASC),
    CONSTRAINT [FK_PaymentGateways_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_PaymentGateways_WebsiteID]
    ON [dbo].[PaymentGateways] ([WebsiteID] ASC);
