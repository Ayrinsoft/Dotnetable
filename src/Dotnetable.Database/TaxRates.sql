CREATE TABLE [dbo].[TaxRates] (
    [TaxRateID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID] INT            NOT NULL,
    [Title]           NVARCHAR (100) NOT NULL,
    [Rate]            DECIMAL (9, 6) NOT NULL,
    [CountryID]       INT            NULL,
    [StateID]         INT            NULL,
    [Priority]        INT NOT NULL,
    [IsActive]        BIT NOT NULL,
    [ApplyToShipping] BIT NOT NULL CONSTRAINT [DF_TaxRates_ApplyToShipping] DEFAULT ((0)),
    [TaxCode]         NVARCHAR (30)  NULL,
    [TaxKind]         TINYINT NOT NULL CONSTRAINT [DF_TaxRates_TaxKind] DEFAULT ((0)),
    CONSTRAINT [PK_TaxRates] PRIMARY KEY CLUSTERED ([TaxRateID] ASC),
    CONSTRAINT [FK_TaxRates_Countries] FOREIGN KEY ([CountryID]) REFERENCES [dbo].[Countries] ([CountryID]),
    CONSTRAINT [FK_TaxRates_States] FOREIGN KEY ([StateID]) REFERENCES [dbo].[States] ([StateID]),
    CONSTRAINT [FK_TaxRates_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

-- Rate is a fraction (0.09 = 9%). Multiple active rows can apply to the same order (stacked
-- taxes); Priority only controls calculation order, not exclusivity.
GO

CREATE NONCLUSTERED INDEX [IX_TaxRates_CountryID]
    ON [dbo].[TaxRates] ([CountryID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_TaxRates_StateID]
    ON [dbo].[TaxRates] ([StateID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_TaxRates_WebsiteID]
    ON [dbo].[TaxRates] ([WebsiteID] ASC);
