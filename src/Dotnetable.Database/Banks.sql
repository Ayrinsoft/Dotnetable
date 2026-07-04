CREATE TABLE [dbo].[Bank] (
    [BankID]     INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]  INT           NULL,
    [Name]       NVARCHAR (64) NOT NULL,
    [LogoFileID] INT           NULL,
    [Active]     BIT           NOT NULL,
    [BankCode]   VARCHAR (10)  NOT NULL,
    CONSTRAINT [PK_Bank] PRIMARY KEY CLUSTERED ([BankID] ASC),
    CONSTRAINT [FK_Bank_FileRecord] FOREIGN KEY ([LogoFileID]) REFERENCES [dbo].[FileRecord] ([FileRecordID]),
    CONSTRAINT [FK_Bank_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

