CREATE TABLE [dbo].[TB_State] (
    [StateID]      INT           IDENTITY (1, 1) NOT NULL,
    [CountryID]    TINYINT       NOT NULL,
    [Tile]         NVARCHAR (48) NOT NULL,
    [LanguageCode] CHAR (2)      NOT NULL,
    [Active]       BIT           NOT NULL,
    CONSTRAINT [PK_TB_State] PRIMARY KEY CLUSTERED ([StateID] ASC),
    CONSTRAINT [FK_TB_State_TB_Country] FOREIGN KEY ([CountryID]) REFERENCES [dbo].[TB_Country] ([CountryID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_State_CountryID]
    ON [dbo].[TB_State]([CountryID] ASC);

