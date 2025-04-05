CREATE TABLE [dbo].[TB_City_Language] (
    [CityLanguageID] INT           IDENTITY (1, 1) NOT NULL,
    [CityID]         INT           NOT NULL,
    [LanguageCode]   CHAR (2)      NOT NULL,
    [Title]          NVARCHAR (48) NOT NULL,
    CONSTRAINT [PK_TB_City_Language] PRIMARY KEY CLUSTERED ([CityLanguageID] ASC),
    CONSTRAINT [FK_TB_City_Language_TB_City] FOREIGN KEY ([CityID]) REFERENCES [dbo].[TB_City] ([CityID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_City_Language_CityID]
    ON [dbo].[TB_City_Language]([CityID] ASC);

