CREATE TABLE [dbo].[WebsiteClientAddresses] (
    [WebsiteClientAddressID] INT               IDENTITY (1, 1) NOT NULL,
    [WebsiteClientID]        INT               NOT NULL,
    [Title]                  NVARCHAR (100)    NULL,
    [ReceiverName]           NVARCHAR (200)    NULL,
    [CountryID]              INT               NULL,
    [CityID]                 INT               NULL,
    [AddressLine]            NVARCHAR (500)    NOT NULL,
    [PostalCode]             NVARCHAR (20)     NULL,
    [Phone]                  NVARCHAR (20)     NULL,
    [IsDefault]              BIT               CONSTRAINT [DF_WebsiteClientAddresses_IsDefault] DEFAULT ((0)) NOT NULL,
    [Latitude]               DECIMAL (9, 6)    NULL,
    [Longitude]              DECIMAL (9, 6)    NULL,
    CONSTRAINT [PK_WebsiteClientAddresses] PRIMARY KEY CLUSTERED ([WebsiteClientAddressID] ASC),
    CONSTRAINT [FK_WebsiteClientAddresses_Cities] FOREIGN KEY ([CityID]) REFERENCES [dbo].[Cities] ([CityID]),
    CONSTRAINT [FK_WebsiteClientAddresses_Countries] FOREIGN KEY ([CountryID]) REFERENCES [dbo].[Countries] ([CountryID]),
    CONSTRAINT [FK_WebsiteClientAddresses_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID])
);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteClientAddresses_CityId]
    ON [dbo].[WebsiteClientAddresses] ([CityId] ASC);

CREATE NONCLUSTERED INDEX [IX_WebsiteClientAddresses_CountryId]
    ON [dbo].[WebsiteClientAddresses] ([CountryId] ASC);

CREATE NONCLUSTERED INDEX [IX_WebsiteClientAddresses_WebsiteClientID]
    ON [dbo].[WebsiteClientAddresses] ([WebsiteClientID] ASC);
