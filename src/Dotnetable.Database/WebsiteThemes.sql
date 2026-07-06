CREATE TABLE [dbo].[WebsiteThemes] (
    [WebsiteThemeID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]      INT            NOT NULL,
    [Name]           NVARCHAR (100) NOT NULL,
    [SettingsJson]   NVARCHAR (MAX) NOT NULL,
    [IsActive]       BIT            CONSTRAINT [DF_WebsiteThemes_IsActive] DEFAULT ((0)) NOT NULL,
    [CreatedAt]      DATETIME       CONSTRAINT [DF_WebsiteThemes_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_WebsiteThemes] PRIMARY KEY CLUSTERED ([WebsiteThemeID] ASC),
    CONSTRAINT [FK_WebsiteThemes_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
