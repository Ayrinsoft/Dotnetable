CREATE TABLE [dbo].[ShippingMethods] (
    [ShippingMethodID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT            NOT NULL,
    [Title]            NVARCHAR (100) NOT NULL,
    [CarrierName]      NVARCHAR (100) NULL,
    [IsActive]         BIT            CONSTRAINT [DF_ShippingMethods_IsActive] DEFAULT ((1)) NOT NULL,
    [SortOrder]        INT            CONSTRAINT [DF_ShippingMethods_SortOrder] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_ShippingMethods] PRIMARY KEY CLUSTERED ([ShippingMethodID] ASC),
    CONSTRAINT [FK_ShippingMethods_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
