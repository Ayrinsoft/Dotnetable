CREATE TABLE [dbo].[Currencies] (
    [CurrencyCode]  CHAR (3)      NOT NULL,
    [Name]          NVARCHAR (50) NOT NULL,
    [Symbol]        NVARCHAR (10) NOT NULL,
    [DecimalDigits] TINYINT       CONSTRAINT [DF_Currencies_DecimalDigits] DEFAULT ((2)) NOT NULL,
    [IsActive]      BIT           CONSTRAINT [DF_Currencies_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Currencies] PRIMARY KEY CLUSTERED ([CurrencyCode] ASC)
);

-- Global master list, same as Countries/States/Cities: shared across every website and only
-- editable from the master website (WebsiteID = 1).
