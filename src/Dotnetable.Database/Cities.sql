CREATE TABLE [dbo].[Cities] (
    [CityID]       INT           IDENTITY (1, 1) NOT NULL,
    [CountryID]    INT           NOT NULL,
    [StateID]      INT           NULL,
    [Title]        NVARCHAR (48) NOT NULL,
    [LanguageCode] CHAR (2)      NOT NULL,
    [Latitude]     FLOAT (53)    NULL,
    [Longitude]    FLOAT (53)    NOT NULL,
    [Active]       BIT           NOT NULL,
    CONSTRAINT [PK_Cities] PRIMARY KEY CLUSTERED ([CityID] ASC),
    CONSTRAINT [FK_Cities_Countries] FOREIGN KEY ([CountryID]) REFERENCES [dbo].[Countries] ([CountryID]),
    CONSTRAINT [FK_Cities_States] FOREIGN KEY ([StateID]) REFERENCES [dbo].[States] ([StateID])
);

-- Global master data, same rule as [dbo].[Countries]: only the master website (WebsiteID = 1) edits this.
GO

CREATE NONCLUSTERED INDEX [IX_Cities_CountryID]
    ON [dbo].[Cities] ([CountryID] ASC);

CREATE NONCLUSTERED INDEX [IX_Cities_StateID]
    ON [dbo].[Cities] ([StateID] ASC);
