CREATE TABLE [dbo].[PostLocations] (
    [PostID]    INT            NOT NULL,
    [Latitude]  FLOAT (53)     NOT NULL,
    [Longitude] FLOAT (53)     NOT NULL,
    [Kind]      TINYINT        CONSTRAINT [DF_PostLocations_Kind] DEFAULT ((0)) NOT NULL,
    [Address]   NVARCHAR (500) NULL,
    [CityID]    INT            NULL,
    CONSTRAINT [PK_PostLocations] PRIMARY KEY CLUSTERED ([PostID] ASC),
    CONSTRAINT [FK_PostLocations_Cities] FOREIGN KEY ([CityID]) REFERENCES [dbo].[Cities] ([CityID]),
    CONSTRAINT [FK_PostLocations_Posts] FOREIGN KEY ([PostID]) REFERENCES [dbo].[Posts] ([PostID]) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_PostLocations_CityID]
    ON [dbo].[PostLocations] ([CityID] ASC);
