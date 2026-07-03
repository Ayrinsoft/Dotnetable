CREATE TABLE [dbo].[WebsiteClientAddresses] (
    [WebsiteClientAddressID] INT               IDENTITY (1, 1) NOT NULL,
    [WebsiteClientID]        INT               NOT NULL,
    [Title]                  NVARCHAR (100)    NULL,
    [ReceiverName]           NVARCHAR (200)    NULL,
    [CountryId]              INT               NULL,
    [CityId]                 INT               NULL,
    [AddressLine]            NVARCHAR (500)    NOT NULL,
    [PostalCode]             NVARCHAR (20)     NULL,
    [Phone]                  NVARCHAR (20)     NULL,
    [IsDefault]              BIT               CONSTRAINT [DF_WebsiteClientAddresses_IsDefault] DEFAULT ((0)) NOT NULL,
    [Location]               [sys].[geography] NULL,
    CONSTRAINT [PK_WebsiteClientAddresses] PRIMARY KEY CLUSTERED ([WebsiteClientAddressID] ASC),
    CONSTRAINT [FK_WebsiteClientAddresses_City] FOREIGN KEY ([CityId]) REFERENCES [dbo].[City] ([CityID]),
    CONSTRAINT [FK_WebsiteClientAddresses_Country] FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Country] ([CountryID]),
    CONSTRAINT [FK_WebsiteClientAddresses_WebsiteClient] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClient] ([WebsiteClientID])
);

