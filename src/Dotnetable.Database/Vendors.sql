CREATE TABLE [dbo].[Vendors] (
    [VendorID]   INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]  INT            NOT NULL,
    [Name]       NVARCHAR (200) NOT NULL,
    [Slug]       NVARCHAR (200) NOT NULL,
    [LogoFileID] INT            NULL,
    [Rating]     DECIMAL (3, 2) CONSTRAINT [DF_Vendors_Rating_1] DEFAULT ((0)) NOT NULL,
    [IsActive]   BIT            CONSTRAINT [DF_Vendors_IsActive_1] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Vendors] PRIMARY KEY CLUSTERED ([VendorID] ASC),
    CONSTRAINT [FK_Vendors_FileRecord] FOREIGN KEY ([LogoFileID]) REFERENCES [dbo].[FileRecord] ([FileRecordID]),
    CONSTRAINT [FK_Vendors_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

