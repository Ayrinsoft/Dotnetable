CREATE TABLE [dbo].[States] (
    [StateID]      INT           IDENTITY (1, 1) NOT NULL,
    [CountryID]    INT           NOT NULL,
    [Title]        NVARCHAR (48) NOT NULL,
    [LanguageCode] CHAR (2)      NOT NULL,
    [Active]       BIT           NOT NULL,
    CONSTRAINT [PK_States] PRIMARY KEY CLUSTERED ([StateID] ASC),
    CONSTRAINT [FK_States_Countries] FOREIGN KEY ([CountryID]) REFERENCES [dbo].[Countries] ([CountryID])
);

