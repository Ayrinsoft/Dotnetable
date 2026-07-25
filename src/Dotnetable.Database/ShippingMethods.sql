CREATE TABLE [dbo].[ShippingMethods] (
    [ShippingMethodID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT            NOT NULL,
    [Title]            NVARCHAR (100) NOT NULL,
    [CarrierName]      NVARCHAR (100) NULL,
    [IsActive]         BIT NOT NULL,
    [SortOrder]        INT NOT NULL,
    CONSTRAINT [PK_ShippingMethods] PRIMARY KEY CLUSTERED ([ShippingMethodID] ASC),
    CONSTRAINT [FK_ShippingMethods_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ShippingMethods_WebsiteID]
    ON [dbo].[ShippingMethods] ([WebsiteID] ASC);
