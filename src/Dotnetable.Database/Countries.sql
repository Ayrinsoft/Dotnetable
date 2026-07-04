CREATE TABLE [dbo].[Countries] (
    [CountryID]    INT           IDENTITY (1, 1) NOT NULL,
    [CountryCode]  CHAR (2)      NOT NULL,
    [LanguageCode] CHAR (2)      NOT NULL,
    [Title]        NVARCHAR (42) NOT NULL,
    [PhonePerfix]  VARCHAR (3)   NOT NULL,
    CONSTRAINT [PK_Countries] PRIMARY KEY CLUSTERED ([CountryID] ASC)
);

-- Global master data (no WebsiteID): shared across every website and only editable from the
-- master website (WebsiteID = 1) - enforced at the application layer, not by this table alone.

