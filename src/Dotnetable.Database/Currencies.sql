CREATE TABLE [dbo].[Currencies] (
    [CurrencyCode]  CHAR (3)      NOT NULL,
    [Name]          NVARCHAR (50) NOT NULL,
    [Symbol]        NVARCHAR (10) NOT NULL,
    [DecimalDigits] TINYINT NOT NULL,
    [IsActive]      BIT NOT NULL,
    CONSTRAINT [PK_Currencies] PRIMARY KEY CLUSTERED ([CurrencyCode] ASC)
);

-- Global master list, same as Countries/States/Cities: shared across every website and only
-- editable from the master website (WebsiteID = 1).
