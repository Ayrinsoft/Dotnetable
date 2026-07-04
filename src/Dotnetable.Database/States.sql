CREATE TABLE [dbo].[State] (
    [StateID]      INT           IDENTITY (1, 1) NOT NULL,
    [CountryID]    INT           NOT NULL,
    [Tile]         NVARCHAR (48) NOT NULL,
    [LanguageCode] CHAR (2)      NOT NULL,
    [Active]       BIT           NOT NULL,
    CONSTRAINT [PK_State] PRIMARY KEY CLUSTERED ([StateID] ASC),
    CONSTRAINT [FK_State_Country] FOREIGN KEY ([CountryID]) REFERENCES [dbo].[Country] ([CountryID])
);

