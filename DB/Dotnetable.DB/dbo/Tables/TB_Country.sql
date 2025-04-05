CREATE TABLE [dbo].[TB_Country] (
    [CountryID]    TINYINT       IDENTITY (1, 1) NOT NULL,
    [CountryCode]  CHAR (2)      NOT NULL,
    [LanguageCode] CHAR (2)      NOT NULL,
    [Title]        NVARCHAR (42) NOT NULL,
    [PhonePerfix]  VARCHAR (3)   NOT NULL,
    CONSTRAINT [PK_TB_Country] PRIMARY KEY CLUSTERED ([CountryID] ASC)
);

