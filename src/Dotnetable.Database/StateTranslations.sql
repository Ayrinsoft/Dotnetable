CREATE TABLE [dbo].[StateTranslation] (
    [StateTranslationID] INT           IDENTITY (1, 1) NOT NULL,
    [Tile]               NVARCHAR (48) NOT NULL,
    [LanguageCode]       CHAR (2)      NOT NULL,
    [StateID]            INT           NOT NULL,
    CONSTRAINT [PK_StateTranslation] PRIMARY KEY CLUSTERED ([StateTranslationID] ASC),
    CONSTRAINT [FK_StateTranslation_State] FOREIGN KEY ([StateID]) REFERENCES [dbo].[State] ([StateID])
);

