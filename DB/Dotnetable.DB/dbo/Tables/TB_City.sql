CREATE TABLE [dbo].[TB_City] (
    [CityID]       INT           IDENTITY (1, 1) NOT NULL,
    [CountryID]    TINYINT       NOT NULL,
    [StateID]      INT           NULL,
    [Title]        NVARCHAR (48) NOT NULL,
    [LanguageCode] CHAR (2)      NOT NULL,
    [Active]       BIT           NOT NULL,
    CONSTRAINT [PK_TB_City] PRIMARY KEY CLUSTERED ([CityID] ASC),
    CONSTRAINT [FK_TB_City_TB_Country] FOREIGN KEY ([CountryID]) REFERENCES [dbo].[TB_Country] ([CountryID]),
    CONSTRAINT [FK_TB_City_TB_State] FOREIGN KEY ([StateID]) REFERENCES [dbo].[TB_State] ([StateID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_City_StateID]
    ON [dbo].[TB_City]([StateID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TB_City_CountryID]
    ON [dbo].[TB_City]([CountryID] ASC);

