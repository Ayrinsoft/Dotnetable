CREATE TABLE [dbo].[Languages] (
    [LanguageID]      INT           IDENTITY (1, 1) NOT NULL,
    [LanguageCode]    CHAR (2)      NOT NULL,
    [LanguageCodeISO] CHAR (5)      NOT NULL,
    [Name]            NVARCHAR (32) NOT NULL,
    [Priority]        INT           NOT NULL,
    [IsDefault]       BIT           NOT NULL,
    [Active]          BIT           NOT NULL,
    [RTLDesign]       BIT           NOT NULL,
    -- NULL = admin panel catalog (UI languages). Non-null = that website's storefront/content languages only.
    [WebsiteID]       INT           NULL,
    CONSTRAINT [PK_Languages] PRIMARY KEY CLUSTERED ([LanguageID] ASC),
    CONSTRAINT [FK_Languages_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

-- Admin catalog: unique code when WebsiteID is null.
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Languages_Admin_LanguageCode]
    ON [dbo].[Languages] ([LanguageCode] ASC)
    WHERE [WebsiteID] IS NULL;
GO

-- Per-website languages: unique code per site.
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Languages_WebsiteID_LanguageCode]
    ON [dbo].[Languages] ([WebsiteID] ASC, [LanguageCode] ASC)
    WHERE [WebsiteID] IS NOT NULL;
