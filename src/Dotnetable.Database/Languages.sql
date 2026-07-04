CREATE TABLE [dbo].[Languages] (
    [LanguageID]      INT          IDENTITY (1, 1) NOT NULL,
    [LanguageCode]    CHAR (2)     NOT NULL,
    [LanguageCodeISO] CHAR (5)     NOT NULL,
    [Name]            VARCHAR (32) NOT NULL,
    [Priority]        INT          NOT NULL,
    [IsDefault]       BIT          NOT NULL,
    [Active]          BIT          NOT NULL,
    [RTLDesign]       BIT          NOT NULL,
    [WebsiteID]       INT          NOT NULL,
    CONSTRAINT [PK_Languages] PRIMARY KEY CLUSTERED ([LanguageID] ASC),
    CONSTRAINT [UQ_Languages_WebsiteID_LanguageCode] UNIQUE ([WebsiteID], [LanguageCode]),
    CONSTRAINT [FK_Languages_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
