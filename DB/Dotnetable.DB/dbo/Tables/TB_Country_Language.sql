CREATE TABLE [dbo].[TB_Country_Language] (
    [CountryLanguageID] SMALLINT      IDENTITY (1, 1) NOT NULL,
    [LanguageCode]      CHAR (2)      NOT NULL,
    [Title]             NVARCHAR (42) NOT NULL,
    [CountryID]         TINYINT       NOT NULL,
    CONSTRAINT [PK_TB_Country_Language] PRIMARY KEY CLUSTERED ([CountryLanguageID] ASC),
    CONSTRAINT [FK_TB_Country_Language_TB_Country] FOREIGN KEY ([CountryID]) REFERENCES [dbo].[TB_Country] ([CountryID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_Country_Language_CountryID]
    ON [dbo].[TB_Country_Language]([CountryID] ASC);

