CREATE TABLE [dbo].[City] (
    [CityID]       INT           IDENTITY (1, 1) NOT NULL,
    [CountryID]    INT           NOT NULL,
    [StateID]      INT           NULL,
    [Title]        NVARCHAR (48) NOT NULL,
    [LanguageCode] CHAR (2)      NOT NULL,
    [Latitude]     FLOAT (53)    NULL,
    [Longitude]    FLOAT (53)    NOT NULL,
    [Active]       BIT           NOT NULL,
    CONSTRAINT [PK_City] PRIMARY KEY CLUSTERED ([CityID] ASC),
    CONSTRAINT [FK_City_Country] FOREIGN KEY ([CountryID]) REFERENCES [dbo].[Country] ([CountryID]),
    CONSTRAINT [FK_City_State] FOREIGN KEY ([StateID]) REFERENCES [dbo].[State] ([StateID])
);

