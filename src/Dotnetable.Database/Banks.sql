CREATE TABLE [dbo].[Banks] (
    [BankID]     INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]  INT           NULL,
    [Name]       NVARCHAR (64) NOT NULL,
    [LogoFileID] INT           NULL,
    [Active]     BIT           NOT NULL,
    [BankCode]   VARCHAR (10)  NOT NULL,
    CONSTRAINT [PK_Banks] PRIMARY KEY CLUSTERED ([BankID] ASC),
    CONSTRAINT [FK_Banks_FileRecords] FOREIGN KEY ([LogoFileID]) REFERENCES [dbo].[FileRecords] ([FileRecordID]),
    CONSTRAINT [FK_Banks_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

