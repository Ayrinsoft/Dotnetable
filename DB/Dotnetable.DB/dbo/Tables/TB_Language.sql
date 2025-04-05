CREATE TABLE [dbo].[TB_Language] (
    [LanguageID]      TINYINT       IDENTITY (1, 1) NOT NULL,
    [Title]           VARCHAR (32)  NOT NULL,
    [LocalizedTitle]  NVARCHAR (32) NOT NULL,
    [LanguageCode]    CHAR (2)      NOT NULL,
    [LanguageISOCode] VARCHAR (5)   NOT NULL,
    [RTLDesign]       BIT           NOT NULL,
    [ItemPriority]    TINYINT       NULL,
    CONSTRAINT [PK_TB_Language] PRIMARY KEY CLUSTERED ([LanguageID] ASC)
);

