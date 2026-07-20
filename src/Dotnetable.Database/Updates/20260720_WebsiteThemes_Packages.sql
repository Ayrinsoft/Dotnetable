/*
  Upgrade WebsiteThemes from design-token JSON rows to WordPress-style package metadata.
  Safe to re-run: checks column existence before altering.
*/

IF COL_LENGTH('dbo.WebsiteThemes', 'SettingsJson') IS NOT NULL
BEGIN
    -- Drop legacy token bag (data is not migrated; packages are file-based now).
    ALTER TABLE [dbo].[WebsiteThemes] DROP COLUMN [SettingsJson];
END
GO

IF COL_LENGTH('dbo.WebsiteThemes', 'Slug') IS NULL
BEGIN
    ALTER TABLE [dbo].[WebsiteThemes] ADD [Slug] NVARCHAR(64) NULL;
    -- Temporary fill for any leftover rows so NOT NULL can be applied.
    EXEC(N'UPDATE [dbo].[WebsiteThemes] SET [Slug] = CONCAT(N''theme-'', [WebsiteThemeID]) WHERE [Slug] IS NULL');
    ALTER TABLE [dbo].[WebsiteThemes] ALTER COLUMN [Slug] NVARCHAR(64) NOT NULL;
END
GO

IF COL_LENGTH('dbo.WebsiteThemes', 'Version') IS NULL
    ALTER TABLE [dbo].[WebsiteThemes] ADD [Version] NVARCHAR(50) NULL;
GO

IF COL_LENGTH('dbo.WebsiteThemes', 'Author') IS NULL
    ALTER TABLE [dbo].[WebsiteThemes] ADD [Author] NVARCHAR(100) NULL;
GO

IF COL_LENGTH('dbo.WebsiteThemes', 'Description') IS NULL
    ALTER TABLE [dbo].[WebsiteThemes] ADD [Description] NVARCHAR(500) NULL;
GO

IF COL_LENGTH('dbo.WebsiteThemes', 'HasScreenshot') IS NULL
    ALTER TABLE [dbo].[WebsiteThemes] ADD [HasScreenshot] BIT NOT NULL
        CONSTRAINT [DF_WebsiteThemes_HasScreenshot] DEFAULT ((0));
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_WebsiteThemes_WebsiteID_Slug'
      AND object_id = OBJECT_ID(N'dbo.WebsiteThemes'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_WebsiteThemes_WebsiteID_Slug]
        ON [dbo].[WebsiteThemes] ([WebsiteID] ASC, [Slug] ASC);
END
GO
