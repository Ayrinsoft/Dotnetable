CREATE TABLE [dbo].[WebsiteThemes] (
    [WebsiteThemeID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]      INT            NOT NULL,
    [Slug]           NVARCHAR (64)  NOT NULL,
    [Name]           NVARCHAR (100) NOT NULL,
    [Version]        NVARCHAR (50)  NULL,
    [Author]         NVARCHAR (100) NULL,
    [Description]    NVARCHAR (500) NULL,
    [HasScreenshot]  BIT NOT NULL,
    [IsActive]       BIT NOT NULL,
    [CreatedAt]      DATETIME NOT NULL,
    CONSTRAINT [PK_WebsiteThemes] PRIMARY KEY CLUSTERED ([WebsiteThemeID] ASC),
    CONSTRAINT [FK_WebsiteThemes_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_WebsiteThemes_WebsiteID_Slug]
    ON [dbo].[WebsiteThemes] ([WebsiteID] ASC, [Slug] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteThemes_WebsiteID]
    ON [dbo].[WebsiteThemes] ([WebsiteID] ASC);
