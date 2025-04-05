CREATE TABLE [dbo].[TB_State_Language] (
    [StateLanguageID] INT           IDENTITY (1, 1) NOT NULL,
    [Tile]            NVARCHAR (48) NOT NULL,
    [LanguageCode]    CHAR (2)      NOT NULL,
    [StateID]         INT           NOT NULL,
    CONSTRAINT [PK_TB_State_Language] PRIMARY KEY CLUSTERED ([StateLanguageID] ASC),
    CONSTRAINT [FK_TB_State_Language_TB_State] FOREIGN KEY ([StateID]) REFERENCES [dbo].[TB_State] ([StateID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TB_State_Language_StateID]
    ON [dbo].[TB_State_Language]([StateID] ASC);

