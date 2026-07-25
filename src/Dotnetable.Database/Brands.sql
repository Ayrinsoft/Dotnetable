CREATE TABLE [dbo].[Brands] (
    [BrandID]    INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]  INT            NOT NULL,
    [Name]       NVARCHAR (200) NOT NULL,
    [Slug]       NVARCHAR (200) NOT NULL,
    [LogoFileID] INT            NULL,
    [IsActive]   BIT NOT NULL,
    CONSTRAINT [PK_Brands] PRIMARY KEY CLUSTERED ([BrandID] ASC),
    CONSTRAINT [FK_Brands_FileRecords] FOREIGN KEY ([LogoFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_Brands_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_Brands_LogoFileID]
    ON [dbo].[Brands] ([LogoFileID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Brands_WebsiteID]
    ON [dbo].[Brands] ([WebsiteID] ASC);
