CREATE TABLE [dbo].[Language] (
    [LangaugeID]      INT          IDENTITY (1, 1) NOT NULL,
    [LanguageCode]    CHAR (2)     NOT NULL,
    [LanguageCodeISO] CHAR (5)     NOT NULL,
    [Name]            VARCHAR (32) NOT NULL,
    [Priority]        INT          NOT NULL,
    [IsDefault]       BIT          NOT NULL,
    [Active]          BIT          NOT NULL,
    [RTLDesign]       BIT          NOT NULL,
    [WebsiteID]       INT          NOT NULL,
    CONSTRAINT [PK_Language] PRIMARY KEY CLUSTERED ([LangaugeID] ASC),
    CONSTRAINT [FK_Language_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

