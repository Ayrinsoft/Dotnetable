CREATE TABLE [dbo].[AuthorVisitCards] (
    [AuthorVisitCardID] INT             IDENTITY (1, 1) NOT NULL,
    [MemberID]          INT             NOT NULL,
    [WebsiteID]         INT             NOT NULL,
    [Enabled]           BIT             CONSTRAINT [DF_AuthorVisitCards_Enabled] DEFAULT ((0)) NOT NULL,
    [Slug]              VARCHAR (32)    NULL,
    [FullName]          NVARCHAR (70)   NULL,
    [BrandName]         NVARCHAR (70)   NULL,
    [JobTitle]          NVARCHAR (60)   NULL,
    [Slogan]            NVARCHAR (160)  NULL,
    [Mobile]            VARCHAR (20)    NULL,
    [Phone]             VARCHAR (20)    NULL,
    [WhatsApp]          VARCHAR (20)    NULL,
    [Address]           NVARCHAR (250)  NULL,
    [WebsiteUrl]        NVARCHAR (200)  NULL,
    [Email]             NVARCHAR (120)  NULL,
    [LogoFileID]        INT             NULL,
    [CoverFileID]       INT             NULL,
    [CardColor]         VARCHAR (20)    NULL,
    [PageColor]         VARCHAR (20)    NULL,
    [Field1Title]       NVARCHAR (50)   NULL,
    [Field1Value]       NVARCHAR (250)  NULL,
    [Field2Title]       NVARCHAR (50)   NULL,
    [Field2Value]       NVARCHAR (250)  NULL,
    [Field3Title]       NVARCHAR (50)   NULL,
    [Field3Value]       NVARCHAR (250)  NULL,
    [Field4Title]       NVARCHAR (50)   NULL,
    [Field4Value]       NVARCHAR (250)  NULL,
    [Field5Title]       NVARCHAR (50)   NULL,
    [Field5Value]       NVARCHAR (250)  NULL,
    [IsPortrait]        BIT             CONSTRAINT [DF_AuthorVisitCards_IsPortrait] DEFAULT ((1)) NOT NULL,
    [CardLayout]        TINYINT         CONSTRAINT [DF_AuthorVisitCards_CardLayout] DEFAULT ((0)) NOT NULL,
    [ThemeMode]         TINYINT         CONSTRAINT [DF_AuthorVisitCards_ThemeMode] DEFAULT ((0)) NOT NULL,
    [Description]       NVARCHAR (4000) NULL,
    [TranslationsJson]  NVARCHAR (MAX)  NULL,
    [SocialLinksJson]   NVARCHAR (MAX)  NULL,
    [LocationsJson]     NVARCHAR (MAX)  NULL,
    [PhoneBookEnabled]  BIT             CONSTRAINT [DF_AuthorVisitCards_PhoneBookEnabled] DEFAULT ((1)) NOT NULL,
    [Bank1HolderName]   NVARCHAR (70)   NULL,
    [Bank1Details]      NVARCHAR (120)  NULL,
    [Bank1Sheba]        VARCHAR (26)    NULL,
    [Bank1CardNumber]   VARCHAR (16)    NULL,
    [Bank2HolderName]   NVARCHAR (70)   NULL,
    [Bank2Details]      NVARCHAR (120)  NULL,
    [Bank2Sheba]        VARCHAR (26)    NULL,
    [Bank2CardNumber]   VARCHAR (16)    NULL,
    [CreatedAt]         DATETIME2 (0)   NOT NULL,
    [UpdatedAt]         DATETIME2 (0)   NOT NULL,
    CONSTRAINT [PK_AuthorVisitCards] PRIMARY KEY CLUSTERED ([AuthorVisitCardID] ASC),
    CONSTRAINT [FK_AuthorVisitCards_Members] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[Members] ([MemberID]) ON DELETE CASCADE,
    CONSTRAINT [FK_AuthorVisitCards_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_AuthorVisitCards_FileRecords_Logo] FOREIGN KEY ([LogoFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_AuthorVisitCards_FileRecords_Cover] FOREIGN KEY ([CoverFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_AuthorVisitCards_MemberID]
    ON [dbo].[AuthorVisitCards] ([MemberID] ASC);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_AuthorVisitCards_WebsiteID_Slug]
    ON [dbo].[AuthorVisitCards] ([WebsiteID] ASC, [Slug] ASC) WHERE ([Slug] IS NOT NULL);
GO

CREATE NONCLUSTERED INDEX [IX_AuthorVisitCards_LogoFileID]
    ON [dbo].[AuthorVisitCards] ([LogoFileID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_AuthorVisitCards_CoverFileID]
    ON [dbo].[AuthorVisitCards] ([CoverFileID] ASC);
GO
