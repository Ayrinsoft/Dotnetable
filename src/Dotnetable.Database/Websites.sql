CREATE TABLE [dbo].[Websites] (
    [WebsiteID]           INT              IDENTITY (1, 1) NOT NULL,
    [TradeName]           NVARCHAR (32)    NOT NULL,
    [WebsiteAddress]      VARCHAR (60)     NOT NULL,
    [AuthCode]            UNIQUEIDENTIFIER NOT NULL,
    [Active]              BIT              NOT NULL,
    [Manager]             NVARCHAR (30)    NOT NULL,
    [Mobile]              VARCHAR (15)     NOT NULL,
    [Email]               VARCHAR (60)     NOT NULL,
    [RegisterDate]        DATE             NOT NULL,
    [AllowAllIP]          BIT              NOT NULL,
    [DefaultLanguageCode] CHAR (2)         NOT NULL,
    [WebsiteType]         TINYINT          NOT NULL,
    [IsHub]               BIT              CONSTRAINT [DF_Websites_IsHub] DEFAULT ((0)) NOT NULL,
    [BrandName]           NVARCHAR (60)    NOT NULL,
    [LogoFileID]          INT              NULL,
    [FaveIconFileID]      INT              NULL,
    [DefaultCurrencyCode] CHAR (3)         NOT NULL,
    CONSTRAINT [PK_Websites] PRIMARY KEY CLUSTERED ([WebsiteID] ASC),
    CONSTRAINT [FK_Websites_Currencies] FOREIGN KEY ([DefaultCurrencyCode]) REFERENCES [dbo].[Currencies] ([CurrencyCode]),
    CONSTRAINT [FK_Websites_FileRecords] FOREIGN KEY ([LogoFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_Websites_FileRecord1] FOREIGN KEY ([FaveIconFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID])
);




GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'show in title of pages', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Websites', @level2type = N'COLUMN', @level2name = N'BrandName';

