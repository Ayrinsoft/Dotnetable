CREATE TABLE [dbo].[Brands] (
    [BrandID]    INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]  INT            NOT NULL,
    [Name]       NVARCHAR (200) NOT NULL,
    [Slug]       NVARCHAR (200) NOT NULL,
    [LogoFileID] INT            NULL,
    [IsActive]   BIT            CONSTRAINT [DF_Brands_IsActive_1] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Brands] PRIMARY KEY CLUSTERED ([BrandID] ASC),
    CONSTRAINT [FK_Brands_FileRecord] FOREIGN KEY ([LogoFileID]) REFERENCES [dbo].[FileRecord] ([FileRecordID]),
    CONSTRAINT [FK_Brands_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

