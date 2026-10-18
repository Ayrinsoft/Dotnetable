CREATE TABLE [dbo].[PaymentGateways] (
    [PaymentGatewayID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT            NOT NULL,
    [Name]             NVARCHAR (150) NOT NULL,
    [Provider]         NVARCHAR (50)  NOT NULL,
    [MerchantID]       NVARCHAR (MAX) NULL,
    [ApiKey]           NVARCHAR (MAX) NULL,
    [ApiSecret]        NVARCHAR (MAX) NULL,
    [IsSandbox]        BIT NOT NULL,
    [SortOrder]        INT NOT NULL,
    [IsActive]         BIT NOT NULL,
    [SettingsJSON]     NVARCHAR (MAX) NULL,
    [CallbackUrl]      NVARCHAR (500)  NULL,
    CONSTRAINT [PK_PaymentGateways] PRIMARY KEY CLUSTERED ([PaymentGatewayID] ASC),
    CONSTRAINT [FK_PaymentGateways_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_PaymentGateways_WebsiteID]
    ON [dbo].[PaymentGateways] ([WebsiteID] ASC);
