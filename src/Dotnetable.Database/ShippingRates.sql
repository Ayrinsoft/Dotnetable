CREATE TABLE [dbo].[ShippingRates] (
    [ShippingRateID]   INT             IDENTITY (1, 1) NOT NULL,
    [ShippingMethodID] INT             NOT NULL,
    [CountryID]        INT             NULL,
    [StateID]          INT             NULL,
    [CityID]           INT             NULL,
    [MinWeightKg]      DECIMAL (10, 3) NULL,
    [MaxWeightKg]      DECIMAL (10, 3) NULL,
    [PriceUsd]         DECIMAL (18, 4) NOT NULL,
    [IsActive]         BIT             CONSTRAINT [DF_ShippingRates_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_ShippingRates] PRIMARY KEY CLUSTERED ([ShippingRateID] ASC),
    CONSTRAINT [FK_ShippingRates_Cities] FOREIGN KEY ([CityID]) REFERENCES [dbo].[Cities] ([CityID]),
    CONSTRAINT [FK_ShippingRates_Countries] FOREIGN KEY ([CountryID]) REFERENCES [dbo].[Countries] ([CountryID]),
    CONSTRAINT [FK_ShippingRates_ShippingMethods] FOREIGN KEY ([ShippingMethodID]) REFERENCES [dbo].[ShippingMethods] ([ShippingMethodID]),
    CONSTRAINT [FK_ShippingRates_States] FOREIGN KEY ([StateID]) REFERENCES [dbo].[States] ([StateID])
);

-- Zone match: NULL on CountryID/StateID/CityID means "any". At runtime pick the row for the
-- destination address whose zone columns are the most specific non-null match.
GO

CREATE NONCLUSTERED INDEX [IX_ShippingRates_CityID]
    ON [dbo].[ShippingRates] ([CityID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ShippingRates_CountryID]
    ON [dbo].[ShippingRates] ([CountryID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ShippingRates_ShippingMethodID]
    ON [dbo].[ShippingRates] ([ShippingMethodID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ShippingRates_StateID]
    ON [dbo].[ShippingRates] ([StateID] ASC);
